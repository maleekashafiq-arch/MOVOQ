import 'package:flutter/material.dart';

String _fmt(int value) {
  final digits = value.toString();
  final buffer = StringBuffer();
  for (var i = 0; i < digits.length; i++) {
    if (i > 0 && (digits.length - i) % 3 == 0) {
      buffer.write(',');
    }
    buffer.write(digits[i]);
  }
  return buffer.toString();
}

class _Prize {
  const _Prize({
    required this.id,
    required this.name,
    required this.emoji,
    required this.ticketCost,
    required this.participants,
    required this.endTime,
    this.featured = false,
  });

  final int id;
  final String name;
  final String emoji;
  final int ticketCost;
  final int participants;
  final String endTime;
  final bool featured;
}

class LuckyDrawScreen extends StatefulWidget {
  const LuckyDrawScreen({super.key, required this.points});

  final int points;

  @override
  State<LuckyDrawScreen> createState() => _LuckyDrawScreenState();
}

class _LuckyDrawScreenState extends State<LuckyDrawScreen> {
  static const _prizes = [
    _Prize(
      id: 1,
      name: 'iPhone 15 Pro',
      emoji: '📱',
      ticketCost: 100,
      participants: 1234,
      endTime: '2 days 14 hours',
      featured: true,
    ),
    _Prize(
      id: 2,
      name: 'AirPods Pro',
      emoji: '🎧',
      ticketCost: 50,
      participants: 892,
      endTime: '5 days 3 hours',
    ),
    _Prize(
      id: 3,
      name: 'Smart Watch',
      emoji: '⌚',
      ticketCost: 75,
      participants: 654,
      endTime: '1 day 8 hours',
    ),
    _Prize(
      id: 4,
      name: 'PKR 10,000 Cash',
      emoji: '💰',
      ticketCost: 150,
      participants: 2156,
      endTime: '3 days 20 hours',
    ),
  ];

  late int _points;
  int? _purchasedId;
  final Map<int, int> _tickets = {};

  @override
  void initState() {
    super.initState();
    _points = widget.points;
  }

  Future<void> _buy(_Prize prize) async {
    if (_points < prize.ticketCost || _purchasedId != null) {
      return;
    }

    setState(() {
      _points -= prize.ticketCost;
      _purchasedId = prize.id;
      _tickets[prize.id] = (_tickets[prize.id] ?? 0) + 1;
    });

    await Future<void>.delayed(const Duration(seconds: 2));
    if (mounted) {
      setState(() => _purchasedId = null);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: DecoratedBox(
        decoration: const BoxDecoration(
          gradient: LinearGradient(
            begin: Alignment.topLeft,
            end: Alignment.bottomRight,
            colors: [Color(0xFFEFF6FF), Color(0xFFECFEFF), Color(0xFFF0FDFA)],
          ),
        ),
        child: Column(
          children: [
            _Header(points: _points),
            Expanded(
              child: ListView(
                padding: const EdgeInsets.fromLTRB(20, 20, 20, 32),
                children: [
                  for (final prize in _prizes) ...[
                    _PrizeCard(
                      prize: prize,
                      points: _points,
                      purchased: _purchasedId == prize.id,
                      ticketsOwned: _tickets[prize.id] ?? 0,
                      onBuy: () => _buy(prize),
                    ),
                    const SizedBox(height: 16),
                  ],
                  const _HowItWorksCard(),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _Header extends StatelessWidget {
  const _Header({required this.points});

  final int points;

  @override
  Widget build(BuildContext context) {
    return DecoratedBox(
      decoration: const BoxDecoration(
        gradient: LinearGradient(colors: [Color(0xFF3B82F6), Color(0xFF06B6D4), Color(0xFF14B8A6)]),
      ),
      child: SafeArea(
        bottom: false,
        child: Padding(
          padding: const EdgeInsets.fromLTRB(16, 12, 16, 20),
          child: Column(
            children: [
              Row(
                children: [
                  IconButton.filled(
                    onPressed: () => Navigator.of(context).pop(),
                    style: IconButton.styleFrom(backgroundColor: Colors.white24, foregroundColor: Colors.white),
                    icon: const Icon(Icons.arrow_back_rounded),
                  ),
                  const Expanded(
                    child: Text(
                      'Lucky Draw',
                      textAlign: TextAlign.center,
                      style: TextStyle(color: Colors.white, fontSize: 22, fontWeight: FontWeight.w900),
                    ),
                  ),
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 12, vertical: 6),
                    decoration: BoxDecoration(color: Colors.white24, borderRadius: BorderRadius.circular(20)),
                    child: Text('${_fmt(points)} pts', style: const TextStyle(color: Colors.white, fontWeight: FontWeight.w700)),
                  ),
                ],
              ),
              const SizedBox(height: 16),
              Container(
                width: double.infinity,
                padding: const EdgeInsets.all(16),
                decoration: BoxDecoration(color: Colors.white24, borderRadius: BorderRadius.circular(20)),
                child: const Row(
                  children: [
                    Icon(Icons.emoji_events_rounded, color: Colors.white, size: 32),
                    SizedBox(width: 12),
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text('Weekly Grand Prize', style: TextStyle(color: Colors.white70)),
                          Text('Win Amazing Rewards!', style: TextStyle(color: Colors.white, fontSize: 18, fontWeight: FontWeight.w800)),
                        ],
                      ),
                    ),
                  ],
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _PrizeCard extends StatelessWidget {
  const _PrizeCard({
    required this.prize,
    required this.points,
    required this.purchased,
    required this.ticketsOwned,
    required this.onBuy,
  });

  final _Prize prize;
  final int points;
  final bool purchased;
  final int ticketsOwned;
  final VoidCallback onBuy;

  @override
  Widget build(BuildContext context) {
    final canBuy = points >= prize.ticketCost && !purchased;

    return Container(
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(28),
        border: prize.featured ? Border.all(color: const Color(0xFF22D3EE), width: 3) : null,
        boxShadow: const [BoxShadow(color: Color(0x14000000), blurRadius: 18, offset: Offset(0, 8))],
      ),
      clipBehavior: Clip.antiAlias,
      child: Column(
        children: [
          if (prize.featured)
            const DecoratedBox(
              decoration: BoxDecoration(
                gradient: LinearGradient(colors: [Color(0xFF3B82F6), Color(0xFF06B6D4), Color(0xFF14B8A6)]),
              ),
              child: Padding(
                padding: EdgeInsets.symmetric(vertical: 8),
                child: Row(
                  mainAxisAlignment: MainAxisAlignment.center,
                  children: [
                    Icon(Icons.auto_awesome, color: Colors.white, size: 16),
                    SizedBox(width: 8),
                    Text('FEATURED PRIZE', style: TextStyle(color: Colors.white, fontWeight: FontWeight.w800, fontSize: 12)),
                    SizedBox(width: 8),
                    Icon(Icons.auto_awesome, color: Colors.white, size: 16),
                  ],
                ),
              ),
            ),
          Padding(
            padding: const EdgeInsets.all(20),
            child: Column(
              children: [
                Row(
                  children: [
                    Container(
                      width: 88,
                      height: 88,
                      decoration: BoxDecoration(
                        borderRadius: BorderRadius.circular(20),
                        gradient: const LinearGradient(colors: [Color(0xFFDBEAFE), Color(0xFFCFFAFE)]),
                      ),
                      child: Center(child: Text(prize.emoji, style: const TextStyle(fontSize: 40))),
                    ),
                    const SizedBox(width: 14),
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(prize.name, style: const TextStyle(fontSize: 20, fontWeight: FontWeight.w900)),
                          const SizedBox(height: 8),
                          Row(
                            children: [
                              const Icon(Icons.groups_rounded, size: 16, color: Color(0xFF06B6D4)),
                              const SizedBox(width: 6),
                              Text('${_fmt(prize.participants)} participants', style: TextStyle(color: Colors.grey.shade600)),
                            ],
                          ),
                          const SizedBox(height: 4),
                          Row(
                            children: [
                              const Icon(Icons.schedule_rounded, size: 16, color: Color(0xFF06B6D4)),
                              const SizedBox(width: 6),
                              Text('Ends in ${prize.endTime}', style: TextStyle(color: Colors.grey.shade600)),
                            ],
                          ),
                          if (ticketsOwned > 0) ...[
                            const SizedBox(height: 4),
                            Text(
                              'Your tickets: $ticketsOwned',
                              style: const TextStyle(color: Color(0xFF0F766E), fontWeight: FontWeight.w700, fontSize: 13),
                            ),
                          ],
                        ],
                      ),
                    ),
                  ],
                ),
                const SizedBox(height: 16),
                Row(
                  children: [
                    Expanded(
                      child: Container(
                        padding: const EdgeInsets.all(12),
                        decoration: BoxDecoration(
                          borderRadius: BorderRadius.circular(16),
                          border: Border.all(color: const Color(0xFFA5F3FC)),
                          gradient: const LinearGradient(colors: [Color(0xFFEFF6FF), Color(0xFFECFEFF)]),
                        ),
                        child: Row(
                          children: [
                            const Icon(Icons.confirmation_number_rounded, color: Color(0xFF0891B2)),
                            const SizedBox(width: 8),
                            Column(
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                Text('Ticket Cost', style: TextStyle(fontSize: 11, color: Colors.grey.shade500)),
                                Text('${prize.ticketCost} points', style: const TextStyle(fontWeight: FontWeight.w900, color: Color(0xFF0E7490))),
                              ],
                            ),
                          ],
                        ),
                      ),
                    ),
                    const SizedBox(width: 12),
                    FilledButton(
                      onPressed: canBuy ? onBuy : null,
                      style: FilledButton.styleFrom(
                        minimumSize: const Size(120, 52),
                        backgroundColor: purchased ? const Color(0xFF2DD4BF) : const Color(0xFF06B6D4),
                        disabledBackgroundColor: purchased ? const Color(0xFF2DD4BF) : const Color(0xFFD1D5DB),
                        disabledForegroundColor: purchased ? Colors.white : Colors.white70,
                      ),
                      child: Text(purchased ? 'Purchased!' : 'Buy Ticket'),
                    ),
                  ],
                ),
                if (points < prize.ticketCost) ...[
                  const SizedBox(height: 8),
                  const Text('Not enough points', style: TextStyle(color: Color(0xFFEF4444), fontSize: 12)),
                ],
              ],
            ),
          ),
        ],
      ),
    );
  }
}

class _HowItWorksCard extends StatelessWidget {
  const _HowItWorksCard();

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(20),
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(28),
        border: Border.all(color: const Color(0xFFA5F3FC)),
        gradient: const LinearGradient(colors: [Color(0xFFEFF6FF), Color(0xFFECFEFF)]),
      ),
      child: const Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Icon(Icons.auto_awesome, color: Color(0xFF0891B2)),
              SizedBox(width: 8),
              Text('How it works', style: TextStyle(fontWeight: FontWeight.w800, fontSize: 16)),
            ],
          ),
          SizedBox(height: 12),
          Text('• Buy tickets with your earned points'),
          SizedBox(height: 6),
          Text('• Each ticket gives you one entry'),
          SizedBox(height: 6),
          Text('• Winners announced when timer ends'),
          SizedBox(height: 6),
          Text('• Buy multiple tickets to increase chances'),
        ],
      ),
    );
  }
}
