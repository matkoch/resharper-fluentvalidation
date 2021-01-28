// using System;
// using System.Diagnostics;
// using JetBrains.Application;
// using JetBrains.Application.Notifications;
// using JetBrains.Application.Settings;
// using JetBrains.Lifetimes;
//
// namespace ReSharperPlugin.FluentValidation.Notifications
// {
//     [ShellComponent]
//     public class SponsorshipNotifier
//     {
//         public SponsorshipNotifier(
//             Lifetime lifetime,
//             UserNotifications userNotifications,
//             ISettingsStore settingsStore)
//         {
//             var settingsStoreLive = settingsStore.BindToContextLive(lifetime, ContextRange.ApplicationWide);
//             var lastNotification =
//                 settingsStoreLive.GetValueProperty(lifetime, (FluentValidationSponsorshipSettings x) => x.LastNotification);
//             if (DateTime.Now.Subtract(TimeSpan.FromMinutes(1)) > lastNotification.Value)
//                 lastNotification.Value = DateTime.Now;
//
//             userNotifications.CreateNotification(
//                 lifetime,
//                 title: "OSS Power-Ups",
//                 body: "Body",
//                 executed: new UserNotificationCommand("Sponsorship website", () =>
//                 {
//                     Process.Start(new ProcessStartInfo
//                     {
//                         FileName = "https://github.com/sponsors/JeremySkinner",
//                         UseShellExecute = true
//                     });
//                 }));
//         }
//     }
// }
