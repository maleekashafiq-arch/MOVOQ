import 'package:flutter/material.dart';

import '../config.dart';
import '../services/session_controller.dart';
import '../theme.dart';
import '../widgets/brand_mark.dart';
import 'sign_up_screen.dart';

class SignInScreen extends StatefulWidget {
  const SignInScreen({super.key, required this.session});

  final SessionController session;

  @override
  State<SignInScreen> createState() => _SignInScreenState();
}

class _SignInScreenState extends State<SignInScreen> {
  final _email = TextEditingController();
  final _password = TextEditingController();
  bool _busy = false;

  @override
  void dispose() {
    _email.dispose();
    _password.dispose();
    super.dispose();
  }

  Future<void> _run(Future<void> Function() action) async {
    setState(() => _busy = true);
    await action();
    if (mounted) {
      setState(() => _busy = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: DecoratedBox(
        decoration: const BoxDecoration(gradient: MovoqTheme.pageGradient),
        child: SafeArea(
          child: ListView(
            padding: const EdgeInsets.fromLTRB(24, 32, 24, 24),
            children: [
              const BrandMark(size: 40),
              const SizedBox(height: 16),
              Text(
                'Welcome back',
                textAlign: TextAlign.center,
                style: Theme.of(context).textTheme.headlineSmall?.copyWith(fontWeight: FontWeight.w900),
              ),
              const SizedBox(height: 6),
              Text(
                'Sign in to keep walking and earning.',
                textAlign: TextAlign.center,
                style: TextStyle(color: Colors.grey.shade600),
              ),
              const SizedBox(height: 28),
              DecoratedBox(
                decoration: BoxDecoration(
                  color: Colors.white,
                  borderRadius: BorderRadius.circular(28),
                  boxShadow: [BoxShadow(color: Colors.black.withValues(alpha: 0.06), blurRadius: 20, offset: const Offset(0, 8))],
                ),
                child: Padding(
                  padding: const EdgeInsets.all(20),
                  child: Column(
                    children: [
                      TextField(
                        controller: _email,
                        keyboardType: TextInputType.emailAddress,
                        autofillHints: const [AutofillHints.email],
                        decoration: const InputDecoration(labelText: 'Email'),
                      ),
                      const SizedBox(height: 12),
                      TextField(
                        controller: _password,
                        obscureText: true,
                        autofillHints: const [AutofillHints.password],
                        decoration: const InputDecoration(labelText: 'Password'),
                      ),
                      if (widget.session.error != null) ...[
                        const SizedBox(height: 12),
                        Text(widget.session.error!, style: const TextStyle(color: Colors.red)),
                      ],
                      const SizedBox(height: 20),
                      FilledButton(
                        onPressed: _busy
                            ? null
                            : () => _run(() => widget.session.signInWithEmail(_email.text, _password.text)),
                        child: Text(_busy ? 'Please wait…' : 'Sign in'),
                      ),
                      const SizedBox(height: 12),
                      OutlinedButton.icon(
                        onPressed: _busy ? null : () => _run(widget.session.signInWithGoogle),
                        icon: const Icon(Icons.g_mobiledata, size: 28),
                        label: const Text('Continue with Google'),
                        style: OutlinedButton.styleFrom(minimumSize: const Size.fromHeight(52)),
                      ),
                    ],
                  ),
                ),
              ),
              const SizedBox(height: 16),
              TextButton(
                onPressed: _busy
                    ? null
                    : () {
                        Navigator.of(context).push(
                          MaterialPageRoute<void>(
                            builder: (_) => SignUpScreen(session: widget.session),
                          ),
                        );
                      },
                child: const Text('New here? Create an account'),
              ),
              if (AppConfig.isLocalDev) ...[
                const SizedBox(height: 8),
                Text(
                  'Local debug: login is optional. Any credentials open Home.',
                  textAlign: TextAlign.center,
                  style: TextStyle(color: Colors.grey.shade600, fontSize: 12),
                ),
                TextButton(
                  onPressed: _busy ? null : () => _run(widget.session.enterLocalPreview),
                  child: const Text('Continue without signing in'),
                ),
              ],
            ],
          ),
        ),
      ),
    );
  }
}
