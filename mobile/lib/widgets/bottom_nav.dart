import 'package:flutter/material.dart';

import '../theme.dart';

class BottomNav extends StatelessWidget {
  const BottomNav({super.key, required this.index, required this.onChanged});

  final int index;
  final ValueChanged<int> onChanged;

  static const items = [
    (Icons.home_rounded, 'Home'),
    (Icons.card_giftcard_rounded, 'Rewards'),
    (Icons.groups_rounded, 'Team'),
    (Icons.person_rounded, 'Profile'),
  ];

  @override
  Widget build(BuildContext context) {
    return DecoratedBox(
      decoration: const BoxDecoration(
        color: Colors.white,
        border: Border(top: BorderSide(color: Color(0xFFE5E7EB))),
      ),
      child: SafeArea(
        top: false,
        child: SizedBox(
          height: 64,
          child: Row(
            children: [
              for (var i = 0; i < items.length; i++)
                Expanded(
                  child: InkWell(
                    onTap: () => onChanged(i),
                    child: Column(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        Icon(
                          items[i].$1,
                          color: i == index
                              ? (i == 2 ? const Color(0xFF0D9488) : MovoqTheme.purple)
                              : Colors.grey.shade400,
                        ),
                        const SizedBox(height: 2),
                        Text(
                          items[i].$2,
                          style: TextStyle(
                            fontSize: 11,
                            fontWeight: i == index ? FontWeight.w700 : FontWeight.w500,
                            color: i == index
                                ? (i == 2 ? const Color(0xFF0D9488) : MovoqTheme.purple)
                                : Colors.grey.shade600,
                          ),
                        ),
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
