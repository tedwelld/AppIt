import { CommonModule } from '@angular/common';
import { Component, OnDestroy, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from './auth.service';
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
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  readonly theme = signal<'light' | 'dark'>('light');
  readonly now = signal(new Date());
  readonly sidebarQuery = signal('');

  readonly currentUser = computed(() => this.auth.user());
  readonly isSuperUser = computed(() => this.auth.isSuperUser());
  readonly panelTitle = computed(() => {
    const role = this.currentUser()?.role;
    if (!role) {
      return 'Welcome';
    }
    return role === 'regular' ? 'Guest Portal' : 'Admin Control Center';
  });
  readonly displayName = computed(() => {
    const user = this.currentUser();
    if (!user) {
      return 'Guest';
    }
    return `${user.firstName} ${user.lastName}`.trim();
  });
  readonly initials = computed(() => {
    const user = this.currentUser();
    if (!user) {
      return 'GU';
    }
    const first = user.firstName?.[0] ?? '';
    const last = user.lastName?.[0] ?? '';
    return `${first}${last}`.toUpperCase() || 'GU';
  });

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

    const role = this.currentUser()?.role;
    if (!role) {
      return [
        {
          id: 'welcome',
          name: 'Welcome',
          icon: 'login',
          items: [{ key: 'auth', title: 'Sign In / Register', icon: 'login', route: '/auth' }]
        }
      ];
    }

    if (role === 'regular') {
      return [
        {
          id: 'user',
          name: 'My Space',
          icon: 'person',
          items: [
            { key: 'user-dashboard', title: 'Dashboard', icon: 'dashboard', route: '/user' },
            { key: 'support', title: 'Support Chat', icon: 'forum', route: '/support' },
            { key: 'settings', title: 'Settings', icon: 'settings', route: '/settings' }
          ]
        }
      ];
    }

    return [
      {
        id: 'overview',
        name: 'Admin Overview',
        icon: 'space_dashboard',
        items: [
          { key: 'dashboard', title: 'Dashboard', icon: 'dashboard', route: '/admin' },
          { key: 'reports', title: 'Reports', icon: 'monitoring', route: '/reports' },
          { key: 'support', title: 'Support Inbox', icon: 'forum', route: '/support' },
          { key: 'settings', title: 'Settings', icon: 'settings', route: '/settings' }
        ]
      },
      {
        id: 'core',
        name: 'Core',
        icon: 'account_tree',
        items: pick(['accounts', 'roles'])
      },
      {
        id: 'operations',
        name: 'Operations',
        icon: 'precision_manufacturing',
        items: pick([
          'products',
          'accommodations',
          'activities',
          'reservations',
          'invoices',
          'payments',
          'vouchers',
          'support-messages'
        ])
      }
    ];
  });

  readonly collapsed = signal<Record<string, boolean>>({
    overview: false,
    core: false,
    operations: false,
    welcome: false,
    user: false
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

  logout(): void {
    this.auth.logout();
    this.router.navigateByUrl('/auth');
  }
}
