import 'package:flutter/material.dart';

import '../config.dart';
import '../services/session_controller.dart';
import '../theme.dart';
import '../widgets/brand_mark.dart';

class SignUpScreen extends StatefulWidget {
  const SignUpScreen({super.key, required this.session});

  final SessionController session;

  @override
  State<SignUpScreen> createState() => _SignUpScreenState();
}

class _SignUpScreenState extends State<SignUpScreen> {
  final _email = TextEditingController();
  final _password = TextEditingController();
  final _confirm = TextEditingController();
  bool _busy = false;
  String? _localError;

  @override
  void dispose() {
    _email.dispose();
    _password.dispose();
    _confirm.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    if (!AppConfig.isLocalDev && _password.text.length < 6) {
      setState(() => _localError = 'Password must be at least 6 characters.');
      return;
    }
    if (!AppConfig.isLocalDev && _password.text != _confirm.text) {
      setState(() => _localError = 'Passwords do not match.');
      return;
    }

    setState(() {
      _localError = null;
      _busy = true;
    });
    await widget.session.signUpWithEmail(_email.text, _password.text);
    if (mounted) {
      setState(() => _busy = false);
      if (widget.session.user != null) {
        Navigator.of(context).pop();
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final error = _localError ?? widget.session.error;
    return Scaffold(
      appBar: AppBar(backgroundColor: Colors.transparent, elevation: 0),
      extendBodyBehindAppBar: true,
      body: DecoratedBox(
        decoration: const BoxDecoration(gradient: MovoqTheme.pageGradient),
        child: SafeArea(
          child: ListView(
            padding: const EdgeInsets.fromLTRB(24, 16, 24, 24),
            children: [
              const BrandMark(size: 32),
              const SizedBox(height: 16),
              Text('Create account', style: Theme.of(context).textTheme.headlineSmall?.copyWith(fontWeight: FontWeight.w900)),
              const SizedBox(height: 8),
              Text('Use email and password. Google can be linked later after the email is verified.',
                  style: TextStyle(color: Colors.grey.shade600)),
              const SizedBox(height: 24),
              TextField(
                controller: _email,
                keyboardType: TextInputType.emailAddress,
                decoration: const InputDecoration(labelText: 'Email'),
              ),
              const SizedBox(height: 12),
              TextField(
                controller: _password,
                obscureText: true,
                decoration: const InputDecoration(labelText: 'Password'),
              ),
              const SizedBox(height: 12),
              TextField(
                controller: _confirm,
                obscureText: true,
                decoration: const InputDecoration(labelText: 'Confirm password'),
              ),
              if (error != null) ...[
                const SizedBox(height: 12),
                Text(error, style: const TextStyle(color: Colors.red)),
              ],
              const SizedBox(height: 20),
              FilledButton(
                onPressed: _busy ? null : _submit,
                child: Text(_busy ? 'Creating…' : 'Create account'),
              ),
              if (AppConfig.isLocalDev) ...[
                const SizedBox(height: 8),
                TextButton(
                  onPressed: _busy
                      ? null
                      : () async {
                          await widget.session.enterLocalPreview();
                          if (context.mounted) {
                            Navigator.of(context).pop();
                          }
                        },
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
