import 'package:flutter/material.dart';

import '../models/reward.dart';
import '../services/api_client.dart';
import '../services/ids.dart';
import '../services/session_controller.dart';

const _categories = ['All', 'Food', 'Shopping', 'Entertainment', 'Travel'];

class RewardsScreen extends StatefulWidget {
  const RewardsScreen({super.key, required this.session});

  final SessionController session;

  @override
  State<RewardsScreen> createState() => _RewardsScreenState();
}

class _RewardsScreenState extends State<RewardsScreen> {
  final _search = TextEditingController();
  MarketplaceList? _list;
  String? _error;
  bool _loading = true;
  String? _redeemingId;
  String _category = 'All';

  int get _points => _list?.walletBalance ?? widget.session.user?.walletBalance ?? 0;

  @override
  void initState() {
    super.initState();
    _search.addListener(() => setState(() {}));
    _load();
  }

  @override
  void dispose() {
    _search.dispose();
    super.dispose();
  }

  List<MarketplaceReward> get _visible {
    final query = _search.text.trim().toLowerCase();
    return (_list?.rewards ?? const <MarketplaceReward>[]).where((reward) {
      if (_category != 'All' && _categoryOf(reward) != _category) {
        return false;
      }
      if (query.isEmpty) {
        return true;
      }
      final haystack = '${reward.sponsorName} ${reward.name} ${reward.description ?? ''}'.toLowerCase();
      return haystack.contains(query);
    }).toList();
  }

  Future<void> _load() async {
    setState(() {
      _loading = true;
      _error = null;
    });
    try {
      final list = await widget.session.api.listRewards(await widget.session.apiToken());
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
          _error = 'Could not load rewards.';
          _loading = false;
        });
      }
    }
  }

  Future<void> _redeem(MarketplaceReward reward) async {
    if (_redeemingId != null || _points < reward.pointsCost) {
      return;
    }
    setState(() => _redeemingId = reward.id);
    try {
      final result = await widget.session.api.redeemReward(
        idToken: await widget.session.apiToken(),
        rewardId: reward.id,
        commandId: newCommandId(),
      );
      widget.session.applyWalletBalance(result.walletBalance);
      await _load();
      if (!mounted) {
        return;
      }
      setState(() => _redeemingId = null);
      final code = result.assignedCode;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(
            result.duplicate
                ? 'Already redeemed.'
                : code == null
                    ? 'Redeemed ${result.rewardName}.'
                    : 'Code $code for ${result.rewardName}.',
          ),
        ),
      );
    } on ApiException catch (e) {
      if (mounted) {
        setState(() => _redeemingId = null);
        ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(e.message)));
      }
    } catch (_) {
      if (mounted) {
        setState(() => _redeemingId = null);
      }
    }
  }

  @override
  Widget build(BuildContext context) {
    final rewards = _visible;
    final mine = _list?.myRedemptions ?? const <MyRedemption>[];

    return DecoratedBox(
      decoration: const BoxDecoration(
        gradient: LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [Color(0xFFF9FAFB), Color(0xFFF3F4F6)],
        ),
      ),
      child: Column(
        children: [
          _MarketplaceHeader(points: _points, search: _search),
          Expanded(
            child: RefreshIndicator(
              onRefresh: _load,
              child: ListView(
                padding: const EdgeInsets.only(bottom: 24),
                children: [
                  SizedBox(
                    height: 64,
                    child: ListView.separated(
                      scrollDirection: Axis.horizontal,
                      padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 12),
                      itemCount: _categories.length,
                      separatorBuilder: (_, _) => const SizedBox(width: 8),
                      itemBuilder: (context, index) {
                        final category = _categories[index];
                        final selected = category == _category;
                        return _CategoryChip(
                          label: category,
                          selected: selected,
                          onTap: () => setState(() => _category = category),
                        );
                      },
                    ),
                  ),
                  if (_loading && _list == null)
                    const Padding(
                      padding: EdgeInsets.symmetric(vertical: 48),
                      child: Center(child: CircularProgressIndicator()),
                    ),
                  if (_error != null)
                    Padding(
                      padding: const EdgeInsets.symmetric(horizontal: 24),
                      child: Column(
                        children: [
                          Text(_error!, style: const TextStyle(color: Color(0xFFEF4444))),
                          TextButton(onPressed: _load, child: const Text('Retry')),
                        ],
                      ),
                    ),
                  if (!_loading && _list != null && rewards.isEmpty)
                    Padding(
                      padding: const EdgeInsets.symmetric(vertical: 48),
                      child: Center(
                        child: Text(
                          _search.text.trim().isEmpty ? 'No offers found in this category' : 'No offers found',
                          style: const TextStyle(color: Color(0xFF6B7280)),
                        ),
                      ),
                    ),
                  for (final reward in rewards)
                    Padding(
                      padding: const EdgeInsets.fromLTRB(24, 0, 24, 16),
                      child: _OfferCard(
                        reward: reward,
                        points: _points,
                        busy: _redeemingId == reward.id,
                        onRedeem: () => _redeem(reward),
                      ),
                    ),
                  if (mine.isNotEmpty) ...[
                    const Padding(
                      padding: EdgeInsets.fromLTRB(24, 8, 24, 8),
                      child: Text('Your redemptions', style: TextStyle(fontSize: 18, fontWeight: FontWeight.w900, color: Color(0xFF1F2937))),
                    ),
                    for (final item in mine)
                      Padding(
                        padding: const EdgeInsets.fromLTRB(24, 0, 24, 12),
                        child: _RedemptionCard(item: item),
                      ),
                  ],
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _MarketplaceHeader extends StatelessWidget {
  const _MarketplaceHeader({required this.points, required this.search});

  final int points;
  final TextEditingController search;

  @override
  Widget build(BuildContext context) {
    return DecoratedBox(
      decoration: const BoxDecoration(
        gradient: LinearGradient(
          begin: Alignment.topLeft,
          end: Alignment.bottomRight,
          colors: [Color(0xFF22C55E), Color(0xFF10B981), Color(0xFF0D9488)],
        ),
      ),
      child: SafeArea(
        bottom: false,
        child: Padding(
          padding: const EdgeInsets.fromLTRB(24, 16, 24, 24),
          child: Column(
            children: [
              Row(
                children: [
                  Container(
                    padding: const EdgeInsets.all(12),
                    decoration: BoxDecoration(color: Colors.white.withValues(alpha: 0.2), shape: BoxShape.circle),
                    child: const Icon(Icons.card_giftcard, color: Colors.white, size: 24),
                  ),
                  const SizedBox(width: 12),
                  const Text('Marketplace', style: TextStyle(color: Colors.white, fontSize: 24, fontWeight: FontWeight.w900)),
                  const Spacer(),
                  Container(
                    padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
                    decoration: BoxDecoration(
                      color: Colors.white.withValues(alpha: 0.2),
                      borderRadius: BorderRadius.circular(999),
                    ),
                    child: Text('$points pts', style: const TextStyle(color: Colors.white, fontWeight: FontWeight.w700)),
                  ),
                ],
              ),
              const SizedBox(height: 16),
              Container(
                padding: const EdgeInsets.symmetric(horizontal: 16),
                decoration: BoxDecoration(
                  color: Colors.white.withValues(alpha: 0.2),
                  borderRadius: BorderRadius.circular(16),
                ),
                child: TextField(
                  controller: search,
                  style: const TextStyle(color: Colors.white),
                  cursorColor: Colors.white,
                  decoration: InputDecoration(
                    icon: Icon(Icons.search, color: Colors.white.withValues(alpha: 0.8)),
                    hintText: 'Search offers...',
                    hintStyle: TextStyle(color: Colors.white.withValues(alpha: 0.6)),
                    border: InputBorder.none,
                    enabledBorder: InputBorder.none,
                    focusedBorder: InputBorder.none,
                    filled: false,
                    isDense: true,
                    contentPadding: const EdgeInsets.symmetric(vertical: 14),
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

class _CategoryChip extends StatelessWidget {
  const _CategoryChip({required this.label, required this.selected, required this.onTap});

  final String label;
  final bool selected;
  final VoidCallback onTap;

  @override
  Widget build(BuildContext context) {
    return Material(
      color: Colors.transparent,
      child: InkWell(
        onTap: onTap,
        borderRadius: BorderRadius.circular(999),
        child: Ink(
          padding: const EdgeInsets.symmetric(horizontal: 16, vertical: 8),
          decoration: BoxDecoration(
            borderRadius: BorderRadius.circular(999),
            color: selected ? null : Colors.white,
            gradient: selected
                ? const LinearGradient(colors: [Color(0xFF22C55E), Color(0xFF059669)])
                : null,
            boxShadow: selected ? const [BoxShadow(color: Color(0x3322C55E), blurRadius: 12, offset: Offset(0, 4))] : null,
          ),
          child: Text(
            label,
            style: TextStyle(
              fontWeight: FontWeight.w600,
              color: selected ? Colors.white : const Color(0xFF4B5563),
            ),
          ),
        ),
      ),
    );
  }
}

class _OfferCard extends StatelessWidget {
  const _OfferCard({
    required this.reward,
    required this.points,
    required this.busy,
    required this.onRedeem,
  });

  final MarketplaceReward reward;
  final int points;
  final bool busy;
  final VoidCallback onRedeem;

  @override
  Widget build(BuildContext context) {
    final canBuy = points >= reward.pointsCost && !busy;
    final discount = _discountLabel(reward.name);
    final until = reward.validUntil;

    return Container(
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(24),
        boxShadow: const [BoxShadow(color: Color(0x1A000000), blurRadius: 16, offset: Offset(0, 8))],
      ),
      padding: const EdgeInsets.all(20),
      child: Column(
        children: [
          Row(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Container(
                width: 80,
                height: 80,
                alignment: Alignment.center,
                decoration: BoxDecoration(
                  borderRadius: BorderRadius.circular(16),
                  gradient: const LinearGradient(
                    begin: Alignment.topLeft,
                    end: Alignment.bottomRight,
                    colors: [Color(0xFFF3E8FF), Color(0xFFFCE7F3)],
                  ),
                ),
                child: Text(_emojiFor(reward), style: const TextStyle(fontSize: 36)),
              ),
              const SizedBox(width: 16),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Expanded(
                          child: Text(
                            reward.sponsorName,
                            style: const TextStyle(fontSize: 18, fontWeight: FontWeight.w900, color: Color(0xFF1F2937)),
                          ),
                        ),
                        if (discount != null) ...[
                          const SizedBox(width: 8),
                          Container(
                            padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                            decoration: BoxDecoration(color: const Color(0xFFEF4444), borderRadius: BorderRadius.circular(8)),
                            child: Text(discount, style: const TextStyle(color: Colors.white, fontSize: 12, fontWeight: FontWeight.w700)),
                          ),
                        ],
                      ],
                    ),
                    const SizedBox(height: 8),
                    Text(reward.name, maxLines: 2, overflow: TextOverflow.ellipsis, style: const TextStyle(color: Color(0xFF4B5563))),
                    if (until != null) ...[
                      const SizedBox(height: 8),
                      Row(
                        children: [
                          const Icon(Icons.local_offer_outlined, size: 12, color: Color(0xFF6B7280)),
                          const SizedBox(width: 6),
                          Text('Valid until ${_formatDate(until)}', style: const TextStyle(fontSize: 12, color: Color(0xFF6B7280))),
                        ],
                      ),
                    ],
                  ],
                ),
              ),
            ],
          ),
          const SizedBox(height: 16),
          const Divider(height: 1, color: Color(0xFFF3F4F6)),
          const SizedBox(height: 16),
          Row(
            children: [
              const Icon(Icons.confirmation_number_outlined, color: Color(0xFF9333EA), size: 20),
              const SizedBox(width: 8),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    const Text('Redeem for', style: TextStyle(fontSize: 12, color: Color(0xFF4B5563))),
                    Text('${reward.pointsCost} pts', style: const TextStyle(fontSize: 18, fontWeight: FontWeight.w900, color: Color(0xFF9333EA))),
                  ],
                ),
              ),
              Opacity(
                opacity: canBuy ? 1 : 0.5,
                child: DecoratedBox(
                  decoration: const BoxDecoration(
                    borderRadius: BorderRadius.all(Radius.circular(999)),
                    gradient: LinearGradient(colors: [Color(0xFF9333EA), Color(0xFFDB2777)]),
                    boxShadow: [BoxShadow(color: Color(0x339333EA), blurRadius: 12, offset: Offset(0, 4))],
                  ),
                  child: Material(
                    color: Colors.transparent,
                    child: InkWell(
                      onTap: canBuy ? onRedeem : null,
                      borderRadius: BorderRadius.circular(999),
                      child: Padding(
                        padding: const EdgeInsets.symmetric(horizontal: 24, vertical: 8),
                        child: Text(
                          busy ? 'Redeeming…' : points < reward.pointsCost ? 'Not enough' : 'Redeem',
                          style: const TextStyle(color: Colors.white, fontWeight: FontWeight.w600),
                        ),
                      ),
                    ),
                  ),
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}

class _RedemptionCard extends StatelessWidget {
  const _RedemptionCard({required this.item});

  final MyRedemption item;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: Colors.white,
        borderRadius: BorderRadius.circular(24),
        boxShadow: const [BoxShadow(color: Color(0x14000000), blurRadius: 12, offset: Offset(0, 4))],
      ),
      child: Row(
        children: [
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(item.rewardName, style: const TextStyle(fontWeight: FontWeight.w800, color: Color(0xFF1F2937))),
                const SizedBox(height: 4),
                Text(
                  [item.sponsorName, if (item.assignedCode != null) item.assignedCode!, item.status].join(' · '),
                  style: const TextStyle(color: Color(0xFF6B7280), fontSize: 13),
                ),
              ],
            ),
          ),
          Text('${item.pointsCost} pts', style: const TextStyle(fontWeight: FontWeight.w800, color: Color(0xFF9333EA))),
        ],
      ),
    );
  }
}

String _categoryOf(MarketplaceReward reward) {
  final text = '${reward.name} ${reward.sponsorName} ${reward.description ?? ''}'.toLowerCase();
  if (RegExp(r'food|meal|pizza|burger|restaurant|cafe|coffee').hasMatch(text)) {
    return 'Food';
  }
  if (RegExp(r'ride|travel|careem|flight|hotel|uber').hasMatch(text)) {
    return 'Travel';
  }
  if (RegExp(r'movie|cinema|entertainment|game|concert').hasMatch(text)) {
    return 'Entertainment';
  }
  if (RegExp(r'shop|khaadi|fashion|voucher|daraz|cloth|wear').hasMatch(text)) {
    return 'Shopping';
  }
  return 'All';
}

String? _discountLabel(String name) {
  final match = RegExp(r'^(\d+%\s*off|bogo|buy 1 get 1|free|pkr\s*\d+)', caseSensitive: false).firstMatch(name.trim());
  if (match == null) {
    return null;
  }
  final label = match.group(1)!;
  if (label.toLowerCase().startsWith('pkr')) {
    return label.toUpperCase().replaceFirst('PKR', 'PKR');
  }
  return label;
}

String _emojiFor(MarketplaceReward reward) {
  final text = '${reward.name} ${reward.sponsorName} ${reward.description ?? ''}'.toLowerCase();
  if (text.contains('pizza')) return '🍕';
  if (text.contains('burger') || text.contains('mcdonald')) return '🍔';
  if (text.contains('ride') || text.contains('careem') || text.contains('taxi')) return '🚗';
  if (text.contains('movie') || text.contains('cinema')) return '🎬';
  if (text.contains('khaadi') || text.contains('fashion') || text.contains('cloth')) return '👗';
  if (text.contains('voucher') || text.contains('daraz') || text.contains('shop')) return '🛍️';
  return '🎁';
}

String _formatDate(DateTime date) {
  const months = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
  final local = date.toLocal();
  return '${months[local.month - 1]} ${local.day}, ${local.year}';
}
