// Parked outside lib/ so the running app does not include Lucky Draw.
import 'package:flutter/material.dart';
import 'package:movoq/services/api_client.dart';
import 'package:movoq/services/ids.dart';
import 'package:movoq/services/session_controller.dart';

import 'lucky_draw.dart';
import 'lucky_draw_api.dart';

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

class LuckyDrawScreen extends StatefulWidget {
  const LuckyDrawScreen({super.key, required this.session});

  final SessionController session;

  @override
  State<LuckyDrawScreen> createState() => _LuckyDrawScreenState();
}

class _LuckyDrawScreenState extends State<LuckyDrawScreen> {
  LuckyDrawList? _list;
  String? _error;
  bool _loading = true;
  String? _buyingId;

  int get _points => _list?.walletBalance ?? widget.session.user?.walletBalance ?? 0;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      final list = await LuckyDrawApi(widget.session.api).listLuckyDraws(await widget.session.apiToken());
      widget.session.applyWalletBalance(list.walletBalance);
      if (mounted) {
        setState(() {
          _list = list;
          _loading = false;
        });
      }
    } on ApiException catch (e) {
      if (mounted) {
        setState(() {
          _error = e.message;
          _loading = false;
        });
      }
    } catch (_) {
      if (mounted) {
        setState(() {
          _error = 'Could not load draws.';
          _loading = false;
        });
      }
    }
  }

  Future<void> _buy(LuckyDraw draw) async {
    if (_buyingId != null || _points < draw.ticketCostPoints) {
      return;
    }

    setState(() => _buyingId = draw.id);
    try {
      final result = await LuckyDrawApi(widget.session.api).buyLuckyDrawTicket(
        idToken: await widget.session.apiToken(),
        drawId: draw.id,
        commandId: newCommandId(),
      );
      widget.session.applyWalletBalance(result.walletBalance);
      if (!mounted) {
        return;
      }
      setState(() {
        _list = LuckyDrawList(
          draws: (_list?.draws ?? const []).map((item) {
            if (item.id != draw.id) {
              return item;
            }
            return item.copyWith(myTickets: result.myTickets, totalTickets: result.totalTickets);
          }).toList(),
          walletBalance: result.walletBalance,
          ticketsBoughtToday: (_list?.ticketsBoughtToday ?? 0) + (result.duplicate ? 0 : 1),
          maxTicketsPerDay: _list?.maxTicketsPerDay ?? 0,
        );
        _buyingId = null;
      });
      final numbers = result.ticketNumbers.isEmpty ? '' : ' Ticket ${result.ticketNumbers.first}.';
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text(result.duplicate ? 'Already recorded.' : 'Ticket bought.$numbers')),
      );
    } on ApiException catch (e) {
      if (mounted) {
        setState(() => _buyingId = null);
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(e.message)));
      }
    } catch (_) {
      if (mounted) {
        setState(() => _buyingId = null);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final draws = _list?.draws ?? const <LuckyDraw>[];

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
              child: RefreshIndicator(
                onRefresh: _load,
                child: ListView(
                  padding: const EdgeInsets.fromLTRB(20, 20, 20, 32),
                  children: [
                    if (_loading && _list == null)
                      const Padding(
                        padding: EdgeInsets.only(top: 48),
                        child: Center(child: CircularProgressIndicator()),
                      ),
                    if (_error != null) ...[
                      Text(_error!, style: const TextStyle(color: Color(0xFFEF4444))),
                      TextButton(onPressed: _load, child: const Text('Retry')),
                    ],
                    if (!_loading && _error == null && draws.isEmpty)
                      const Text('No open draws right now. Create one in the admin app.'),
                    for (final draw in draws) ...[
                      _PrizeCard(
                        draw: draw,
                        points: _points,
                        buying: _buyingId == draw.id,
                        onBuy: () => _buy(draw),
                      ),
                      const SizedBox(height: 16),
                    ],
                    const _HowItWorksCard(),
                  ],
                ),
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
                          Text('Open draws', style: TextStyle(color: Colors.white70)),
                          Text('Buy tickets with your points', style: TextStyle(color: Colors.white, fontSize: 18, fontWeight: FontWeight.w800)),
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
    required this.draw,
    required this.points,
    required this.buying,
    required this.onBuy,
  });

  final LuckyDraw draw;
  final int points;
  final bool buying;
  final VoidCallback onBuy;

  @override
  Widget build(BuildContext context) {
    final canBuy = points >= draw.ticketCostPoints && !buying;

    return Container(
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(28),
        border: draw.featured ? Border.all(color: const Color(0xFF22D3EE), width: 3) : null,
        boxShadow: const [BoxShadow(color: Color(0x14000000), blurRadius: 18, offset: Offset(0, 8))],
      ),
      clipBehavior: Clip.antiAlias,
      child: Column(
        children: [
          if (draw.featured)
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
                      child: Center(child: Text(draw.emoji ?? '🎁', style: const TextStyle(fontSize: 40))),
                    ),
                    const SizedBox(width: 14),
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(draw.name, style: const TextStyle(fontSize: 20, fontWeight: FontWeight.w900)),
                          const SizedBox(height: 4),
                          Text(draw.prizeDescription, style: TextStyle(color: Colors.grey.shade600)),
                          const SizedBox(height: 8),
                          Row(
                            children: [
                              const Icon(Icons.groups_rounded, size: 16, color: Color(0xFF06B6D4)),
                              const SizedBox(width: 6),
                              Text('${_fmt(draw.participants)} participants', style: TextStyle(color: Colors.grey.shade600)),
                            ],
                          ),
                          const SizedBox(height: 4),
                          Row(
                            children: [
                              const Icon(Icons.schedule_rounded, size: 16, color: Color(0xFF06B6D4)),
                              const SizedBox(width: 6),
                              Text('Ends in ${draw.endsIn}', style: TextStyle(color: Colors.grey.shade600)),
                            ],
                          ),
                          if (draw.myTickets > 0) ...[
                            const SizedBox(height: 4),
                            Text(
                              'Your tickets: ${draw.myTickets}',
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
                                Text('${draw.ticketCostPoints} points', style: const TextStyle(fontWeight: FontWeight.w900, color: Color(0xFF0E7490))),
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
                        backgroundColor: buying ? const Color(0xFF2DD4BF) : const Color(0xFF06B6D4),
                        disabledBackgroundColor: buying ? const Color(0xFF2DD4BF) : const Color(0xFFD1D5DB),
                        disabledForegroundColor: buying ? Colors.white : Colors.white70,
                      ),
                      child: Text(buying ? 'Buying…' : 'Buy Ticket'),
                    ),
                  ],
                ),
                if (points < draw.ticketCostPoints) ...[
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
          Text('• Winners announced when the draw closes'),
          SizedBox(height: 6),
          Text('• Buy multiple tickets to increase chances'),
        ],
      ),
    );
  }
}
