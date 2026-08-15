import 'package:flutter/material.dart';

import '../config.dart';
import '../services/session_controller.dart';
import '../theme.dart';

class CompleteProfileScreen extends StatefulWidget {
  const CompleteProfileScreen({super.key, required this.session});

  final SessionController session;

  @override
  State<CompleteProfileScreen> createState() => _CompleteProfileScreenState();
}

class _CompleteProfileScreenState extends State<CompleteProfileScreen> {
  final _name = TextEditingController();
  final _state = TextEditingController();
  final _city = TextEditingController();
  final _phone = TextEditingController();
  String _country = 'PK';
  String? _gender;
  int _goal = 6000;
  bool _busy = false;

  static const _countries = {
    'PK': 'Pakistan',
    'IN': 'India',
    'BD': 'Bangladesh',
    'AE': 'United Arab Emirates',
    'SA': 'Saudi Arabia',
  };

  @override
  void dispose() {
    _name.dispose();
    _state.dispose();
    _city.dispose();
    _phone.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    setState(() => _busy = true);
    await widget.session.completeProfile(
      fullName: _name.text,
      country: _country,
      state: _state.text,
      city: _city.text,
      dailyTargetSteps: _goal,
      phoneNumber: _phone.text.trim().isEmpty ? null : _phone.text.trim(),
      gender: _gender,
    );
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
            padding: const EdgeInsets.fromLTRB(24, 24, 24, 24),
            children: [
              Text('Create your profile', style: Theme.of(context).textTheme.headlineSmall?.copyWith(fontWeight: FontWeight.w900)),
              const SizedBox(height: 6),
              Text('Just the essentials to get you started.', style: TextStyle(color: Colors.grey.shade600)),
              const SizedBox(height: 24),
              DecoratedBox(
                decoration: BoxDecoration(
                  color: Colors.white,
                  borderRadius: BorderRadius.circular(28),
                  boxShadow: [BoxShadow(color: Colors.black.withValues(alpha: 0.06), blurRadius: 20, offset: const Offset(0, 8))],
                ),
                child: Padding(
                  padding: const EdgeInsets.all(20),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
              TextField(controller: _name, decoration: const InputDecoration(labelText: 'Full name')),
              const SizedBox(height: 12),
              DropdownButtonFormField<String>(
                initialValue: _country,
                decoration: const InputDecoration(labelText: 'Country'),
                items: _countries.entries
                    .map((e) => DropdownMenuItem(value: e.key, child: Text(e.value)))
                    .toList(),
                onChanged: (value) => setState(() => _country = value ?? 'PK'),
              ),
              const SizedBox(height: 12),
              TextField(controller: _state, decoration: const InputDecoration(labelText: 'State / province')),
              const SizedBox(height: 12),
              TextField(controller: _city, decoration: const InputDecoration(labelText: 'City')),
              const SizedBox(height: 12),
              TextField(
                controller: _phone,
                keyboardType: TextInputType.phone,
                decoration: const InputDecoration(labelText: 'Phone (optional)'),
              ),
              const SizedBox(height: 12),
              DropdownButtonFormField<String>(
                initialValue: _gender,
                decoration: const InputDecoration(labelText: 'Gender (optional)'),
                items: const [
                  DropdownMenuItem(value: 'Female', child: Text('Female')),
                  DropdownMenuItem(value: 'Male', child: Text('Male')),
                  DropdownMenuItem(value: 'Other', child: Text('Other')),
                ],
                onChanged: (value) => setState(() => _gender = value),
              ),
              const SizedBox(height: 20),
              Text('Daily step goal', style: Theme.of(context).textTheme.titleMedium),
              const SizedBox(height: 8),
              Wrap(
                spacing: 8,
                children: [
                  _GoalChip(label: 'Casual 4k', value: 4000, group: _goal, onSelected: (v) => setState(() => _goal = v)),
                  _GoalChip(label: 'Active 6k', value: 6000, group: _goal, onSelected: (v) => setState(() => _goal = v)),
                  _GoalChip(label: 'Pro 10k', value: 10000, group: _goal, onSelected: (v) => setState(() => _goal = v)),
                ],
              ),
              if (widget.session.error != null) ...[
                const SizedBox(height: 12),
                Text(widget.session.error!, style: const TextStyle(color: Colors.red)),
              ],
              const SizedBox(height: 24),
              FilledButton(
                onPressed: _busy ? null : _submit,
                child: Text(_busy ? 'Saving…' : 'Continue'),
              ),
              if (AppConfig.isLocalDev) ...[
                const SizedBox(height: 8),
                TextButton(
                  onPressed: _busy ? null : widget.session.enterLocalPreview,
                  child: const Text('Skip for now (local)'),
                ),
              ],
                    ],
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _GoalChip extends StatelessWidget {
  const _GoalChip({
    required this.label,
    required this.value,
    required this.group,
    required this.onSelected,
  });

  final String label;
  final int value;
  final int group;
  final ValueChanged<int> onSelected;

  @override
  Widget build(BuildContext context) {
    return ChoiceChip(
      label: Text(label),
      selected: group == value,
      onSelected: (_) => onSelected(value),
    );
  }
}
