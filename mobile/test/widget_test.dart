import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:movoq/screens/onboarding_screen.dart';

void main() {
  testWidgets('onboarding walks through every slide and finishes', (tester) async {
    var done = false;
    await tester.pumpWidget(MaterialApp(home: OnboardingScreen(onDone: () => done = true)));

    expect(find.text('Walk & Earn Points'), findsOneWidget);

    await tester.tap(find.text('Next'));
    await tester.pumpAndSettle();
    expect(find.text('Watch Ads to Boost Rewards'), findsOneWidget);

    await tester.tap(find.text('Next'));
    await tester.pumpAndSettle();
    expect(find.text('Redeem Points & Win Prizes'), findsOneWidget);

    await tester.tap(find.text('Get Started'));
    await tester.pump();
    expect(done, isTrue);
  });
}
