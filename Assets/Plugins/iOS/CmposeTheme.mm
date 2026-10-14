#import <UIKit/UIKit.h>

// Called from IosSystemDarkMode (C#): 1 when the system appearance is dark
extern "C" int CmposeSystemDarkMode(void)
{
    return UIScreen.mainScreen.traitCollection.userInterfaceStyle == UIUserInterfaceStyleDark ? 1 : 0;
}
