from __future__ import annotations
from dataclasses import dataclass, field
from typing import Dict

@dataclass
class DayPolicy:
    max_trades: int = 8          # per calendar day
    dd_limit_r: float = -5.0     # stop trading day if cumR <= this
    _count: int = 0
    _cum_r: float = 0.0

    def can_enter(self) -> bool:
        return self._count < self.max_trades and self._cum_r > self.dd_limit_r

    def register(self, r_mult: float) -> None:
        self._count += 1
        self._cum_r += r_mult

@dataclass
class DailyBook:
    policy_template: DayPolicy = field(default_factory=DayPolicy)
    days: Dict[str, DayPolicy] = field(default_factory=dict)

    def policy_for(self, day_key: str) -> DayPolicy:
        if day_key not in self.days:
            self.days[day_key] = DayPolicy(
                max_trades=self.policy_template.max_trades,
                dd_limit_r=self.policy_template.dd_limit_r
            )
        return self.days[day_key]
