import { CommonModule } from '@angular/common';
import { Component, OnDestroy, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { RESOURCE_CONFIGS } from './entity-config';

interface NavItem {
  key: string;
  title: string;
  icon: string;
  route: string;
}

interface NavGroup {
  id: string;
  name: string;
  icon: string;
  items: NavItem[];
}

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnDestroy {
  readonly theme = signal<'light' | 'dark'>('light');
  readonly now = signal(new Date());
  readonly sidebarQuery = signal('');

  readonly navGroups = computed<NavGroup[]>(() => {
    const byKey = new Map(RESOURCE_CONFIGS.map((item) => [item.key, item]));
    const pick = (keys: string[]) =>
      keys
        .map((key) => byKey.get(key))
        .filter((item): item is (typeof RESOURCE_CONFIGS)[number] => !!item)
        .map((item) => ({
          key: item.key,
          title: item.title,
          icon: item.icon,
          route: `/entities/${item.key}`
        }));

    return [
      {
        id: 'overview',
        name: 'Overview',
        icon: 'space_dashboard',
        items: [
          { key: 'dashboard', title: 'Dashboard', icon: 'dashboard', route: '/dashboard' },
          { key: 'reports', title: 'Reports', icon: 'monitoring', route: '/reports' }
        ]
      },
      {
        id: 'core',
        name: 'Core',
        icon: 'account_tree',
        items: pick(['accounts', 'roles', 'companies', 'departments'])
      },
      {
        id: 'operations',
        name: 'Operations',
        icon: 'precision_manufacturing',
        items: pick(['products', 'customers', 'customer-types', 'reservations', 'invoices', 'suppliers'])
      },
      {
        id: 'access',
        name: 'Access',
        icon: 'security',
        items: pick(['permissions', 'features', 'feature-permissions', 'role-features', 'audit-logs'])
      }
    ];
  });

  readonly collapsed = signal<Record<string, boolean>>({
    overview: false,
    core: false,
    operations: false,
    access: false
  });

  readonly filteredGroups = computed(() => {
    const query = this.sidebarQuery().trim().toLowerCase();
    if (!query) {
      return this.navGroups();
    }

    return this.navGroups()
      .map((group) => ({
        ...group,
        items: group.items.filter((item) => item.title.toLowerCase().includes(query))
      }))
      .filter((group) => group.items.length > 0 || group.name.toLowerCase().includes(query));
  });

  private readonly timer = setInterval(() => this.now.set(new Date()), 1000);

  toggleTheme(): void {
    this.theme.set(this.theme() === 'light' ? 'dark' : 'light');
  }

  toggleGroup(groupId: string): void {
    this.collapsed.update((state) => ({ ...state, [groupId]: !state[groupId] }));
  }

  isCollapsed(groupId: string): boolean {
    return !!this.collapsed()[groupId];
  }

  ngOnDestroy(): void {
    clearInterval(this.timer);
  }
}
