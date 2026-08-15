# MOVOQ mobile

Flutter app for Android and iOS. Feature 1 is login: Firebase Auth, then the .NET identity API.

## One-time Firebase connect

1. In [Firebase console](https://console.firebase.google.com) open `movoq-cc5b8`.
2. Project settings → **Your apps** → add **Android** with package `com.movoq.movoq`. Download `google-services.json` into `android/app/`.
3. Add **iOS** when you are ready and put `GoogleService-Info.plist` in `ios/Runner/`.
4. Copy the **Web API key**, **App ID**, and **Sender ID** from Project settings.

Run with those values (PowerShell):

```powershell
cd e:\MOVOQ\mobile
flutter pub get
flutter run --dart-define=FIREBASE_API_KEY=YOUR_WEB_API_KEY --dart-define=FIREBASE_APP_ID=YOUR_ANDROID_APP_ID --dart-define=FIREBASE_SENDER_ID=YOUR_SENDER_ID
```

Or generate `lib/firebase_options.dart` with:

```powershell
dart pub global activate flutterfire_cli
flutterfire configure --project=movoq-cc5b8
```

Then switch `main.dart` to `DefaultFirebaseOptions.currentPlatform`.

## API

Keep the backend running on `http://localhost:5014`. The Android emulator uses `http://10.0.2.2:5014`.

Override if needed:

```powershell
flutter run --dart-define=API_BASE_URL=http://192.168.1.10:5014
```
