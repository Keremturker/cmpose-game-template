#import <AppTrackingTransparency/AppTrackingTransparency.h>

// Called from IosTrackingAuthorization (C#). The system answers the prompt on its own queue, so C# polls
// CmposeTrackingDone instead of receiving a callback.
static volatile int cmposeTrackingDone = 1;

// ATTrackingManagerAuthorizationStatus: 0 not determined, 1 restricted, 2 denied, 3 authorized
extern "C" int CmposeTrackingStatus(void)
{
    return (int)[ATTrackingManager trackingAuthorizationStatus];
}

extern "C" void CmposeTrackingRequest(void)
{
    cmposeTrackingDone = 0;
    dispatch_async(dispatch_get_main_queue(), ^{
        [ATTrackingManager requestTrackingAuthorizationWithCompletionHandler:^(ATTrackingManagerAuthorizationStatus status) {
            cmposeTrackingDone = 1;
        }];
    });
}

extern "C" int CmposeTrackingDone(void)
{
    return cmposeTrackingDone;
}
