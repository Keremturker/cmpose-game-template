#import <UIKit/UIKit.h>

// Called from IosVibrator (C#): 0 light, 1 medium, 2 heavy, matching HapticStrength
extern "C" void CmposeHapticImpact(int strength)
{
    UIImpactFeedbackStyle style = UIImpactFeedbackStyleLight;
    if (strength == 1) style = UIImpactFeedbackStyleMedium;
    else if (strength >= 2) style = UIImpactFeedbackStyleHeavy;
    UIImpactFeedbackGenerator *generator = [[UIImpactFeedbackGenerator alloc] initWithStyle:style];
    [generator prepare];
    [generator impactOccurred];
#if !__has_feature(objc_arc)
    [generator release];
#endif
}
