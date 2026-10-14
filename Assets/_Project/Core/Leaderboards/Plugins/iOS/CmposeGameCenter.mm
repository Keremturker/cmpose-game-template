#import <GameKit/GameKit.h>
#import <UIKit/UIKit.h>

// Called from IosGameServicesGateway (C#), on Unity's main thread. GameKit answers on its own queues, so C# polls
// instead of receiving callbacks: CmposeGcAuthState for the sign-in and CmposeGcResult(token) for each operation.

static NSObject* cmposeGcLock = [NSObject new];
static NSMutableDictionary<NSNumber*, NSNumber*>* cmposeGcResults = [NSMutableDictionary new];
static int cmposeGcNextToken = 1;
static int cmposeGcAuthState = 0;
static BOOL cmposeGcStarted = NO;
static UIViewController* cmposeGcLoginController = nil;
static BOOL cmposeGcLoginShowing = NO;

@interface CmposeGcDelegate : NSObject <GKGameCenterControllerDelegate>
@end

@implementation CmposeGcDelegate
- (void)gameCenterViewControllerDidFinish:(GKGameCenterViewController*)gameCenterViewController
{
    [gameCenterViewController dismissViewControllerAnimated:YES completion:nil];
}
@end

static CmposeGcDelegate* cmposeGcDelegate = nil;

static int CmposeGcNewToken(void)
{
    @synchronized (cmposeGcLock)
    {
        return cmposeGcNextToken++;
    }
}

static void CmposeGcFinish(int token, BOOL ok)
{
    @synchronized (cmposeGcLock)
    {
        cmposeGcResults[@(token)] = @(ok ? 1 : 2);
    }
}

static void CmposeGcOnMain(void (^block)(void))
{
    if ([NSThread isMainThread]) block();
    else dispatch_async(dispatch_get_main_queue(), block);
}

static NSString* CmposeGcString(const char* value)
{
    return value != NULL ? [NSString stringWithUTF8String:value] : @"";
}

// The topmost view controller of the key window (Unity's root, or whatever is presented over it)
static UIViewController* CmposeGcTopViewController(void)
{
    UIViewController* top = nil;
    for (UIScene* scene in UIApplication.sharedApplication.connectedScenes)
    {
        if (![scene isKindOfClass:[UIWindowScene class]]) continue;
        for (UIWindow* window in ((UIWindowScene*)scene).windows)
        {
            if (window.isKeyWindow)
            {
                top = window.rootViewController;
                break;
            }
        }
        if (top != nil) break;
    }
    if (top == nil)
    {
        id<UIApplicationDelegate> application = UIApplication.sharedApplication.delegate;
        if ([application respondsToSelector:@selector(window)]) top = application.window.rootViewController;
    }
    while (top.presentedViewController != nil) top = top.presentedViewController;
    return top;
}

static void CmposeGcPresent(GKGameCenterViewController* controller, int token)
{
    UIViewController* top = CmposeGcTopViewController();
    if (top == nil)
    {
        CmposeGcFinish(token, NO);
        return;
    }
    if (cmposeGcDelegate == nil) cmposeGcDelegate = [CmposeGcDelegate new];
    controller.gameCenterDelegate = cmposeGcDelegate;
    [top presentViewController:controller animated:YES completion:nil];
    CmposeGcFinish(token, YES);
}

// Reports an achievement's percentage computed from the current one (loaded first, so progress adds up)
static void CmposeGcReport(int token, NSString* identifier, double (^percent)(double current))
{
    [GKAchievement loadAchievementsWithCompletionHandler:^(NSArray<GKAchievement*>* achievements, NSError* error) {
        if (error != nil)
        {
            CmposeGcFinish(token, NO);
            return;
        }
        GKAchievement* achievement = nil;
        for (GKAchievement* candidate in achievements)
        {
            if ([candidate.identifier isEqualToString:identifier]) achievement = candidate;
        }
        if (achievement == nil) achievement = [[GKAchievement alloc] initWithIdentifier:identifier];
        achievement.percentComplete = MIN(100.0, percent(achievement.percentComplete));
        achievement.showsCompletionBanner = YES;
        [GKAchievement reportAchievements:@[ achievement ] withCompletionHandler:^(NSError* reportError) {
            CmposeGcFinish(token, reportError == nil);
        }];
    }];
}

extern "C" void CmposeGcStart(void)
{
    if (cmposeGcStarted) return;
    cmposeGcStarted = YES;
    CmposeGcOnMain(^{
        // GameKit calls the handler at start and again when the player finishes the sign-in screen. A controller it
        // hands over is only kept here: it is shown when the player asks (CmposeGcShowLogin).
        GKLocalPlayer.localPlayer.authenticateHandler = ^(UIViewController* controller, NSError* error) {
            if (controller != nil)
            {
                cmposeGcLoginController = controller;
                cmposeGcAuthState = 2;
            }
            else
            {
                cmposeGcLoginController = nil;
                cmposeGcAuthState = GKLocalPlayer.localPlayer.isAuthenticated ? 1 : 2;
            }
        };
    });
}

extern "C" int CmposeGcAuthState(void)
{
    if (cmposeGcStarted && GKLocalPlayer.localPlayer.isAuthenticated) return 1;
    return cmposeGcAuthState;
}

extern "C" int CmposeGcNeedsLogin(void)
{
    return cmposeGcLoginController != nil ? 1 : 0;
}

extern "C" void CmposeGcShowLogin(void)
{
    UIViewController* controller = cmposeGcLoginController;
    if (controller == nil) return;
    cmposeGcLoginShowing = YES;
    CmposeGcOnMain(^{
        UIViewController* top = CmposeGcTopViewController();
        if (top == nil)
        {
            cmposeGcLoginShowing = NO;
            return;
        }
        [top presentViewController:controller animated:YES completion:nil];
    });
}

extern "C" int CmposeGcLoginVisible(void)
{
    if (!cmposeGcLoginShowing) return 0;
    UIViewController* controller = cmposeGcLoginController;
    if (controller == nil || (controller.presentingViewController == nil && !controller.isBeingPresented))
    {
        cmposeGcLoginShowing = NO;
        return 0;
    }
    return 1;
}

extern "C" int CmposeGcSubmitScore(const char* leaderboardId, long long score)
{
    int token = CmposeGcNewToken();
    NSString* identifier = CmposeGcString(leaderboardId);
    if (!GKLocalPlayer.localPlayer.isAuthenticated)
    {
        CmposeGcFinish(token, NO);
        return token;
    }
    [GKLeaderboard submitScore:(NSInteger)score
                       context:0
                        player:GKLocalPlayer.localPlayer
                leaderboardIDs:@[ identifier ]
             completionHandler:^(NSError* error) {
                 CmposeGcFinish(token, error == nil);
             }];
    return token;
}

extern "C" int CmposeGcUnlock(const char* achievementId)
{
    int token = CmposeGcNewToken();
    if (!GKLocalPlayer.localPlayer.isAuthenticated)
    {
        CmposeGcFinish(token, NO);
        return token;
    }
    CmposeGcReport(token, CmposeGcString(achievementId), ^double(double current) {
        return 100.0;
    });
    return token;
}

extern "C" int CmposeGcIncrement(const char* achievementId, int steps, int totalSteps)
{
    int token = CmposeGcNewToken();
    if (!GKLocalPlayer.localPlayer.isAuthenticated || steps <= 0 || totalSteps <= 0)
    {
        CmposeGcFinish(token, NO);
        return token;
    }
    CmposeGcReport(token, CmposeGcString(achievementId), ^double(double current) {
        return current + 100.0 * steps / totalSteps;
    });
    return token;
}

extern "C" int CmposeGcShowLeaderboard(const char* leaderboardId)
{
    int token = CmposeGcNewToken();
    NSString* identifier = CmposeGcString(leaderboardId);
    CmposeGcOnMain(^{
        GKGameCenterViewController* controller =
            [[GKGameCenterViewController alloc] initWithLeaderboardID:identifier
                                                          playerScope:GKLeaderboardPlayerScopeGlobal
                                                            timeScope:GKLeaderboardTimeScopeAllTime];
        CmposeGcPresent(controller, token);
    });
    return token;
}

extern "C" int CmposeGcShowAchievements(void)
{
    int token = CmposeGcNewToken();
    CmposeGcOnMain(^{
        GKGameCenterViewController* controller =
            [[GKGameCenterViewController alloc] initWithState:GKGameCenterViewControllerStateAchievements];
        CmposeGcPresent(controller, token);
    });
    return token;
}

extern "C" int CmposeGcResult(int token)
{
    @synchronized (cmposeGcLock)
    {
        NSNumber* value = cmposeGcResults[@(token)];
        if (value == nil) return 0;
        [cmposeGcResults removeObjectForKey:@(token)];
        return value.intValue;
    }
}
