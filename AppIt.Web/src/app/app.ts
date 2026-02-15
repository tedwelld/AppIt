import { CommonModule } from '@angular/common';
import { Component, HostListener, OnDestroy, computed, inject, signal } from '@angular/core';
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
  private readonly sidebarStateKey = 'appit.ui.sidebar.open';
  readonly theme = signal<'light' | 'dark'>('light');
  readonly now = signal(new Date());
  readonly welcome = signal('');
  readonly sidebarOpen = signal(false);

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
          items: [{ key: 'auth', title: 'Sign In', icon: 'login', route: '/auth' }]
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

  private readonly timer = setInterval(() => this.now.set(new Date()), 1000);

  constructor() {
    this.welcome.set(this.auth.consumeWelcomeMessage());
    this.restoreSidebarState();
  }

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

  dismissWelcome(): void {
    this.welcome.set('');
  }

  toggleSidebar(): void {
    this.sidebarOpen.update((isOpen) => !isOpen);
    this.persistSidebarState();
  }

  closeSidebar(): void {
    if (!this.sidebarOpen()) {
      return;
    }

    this.sidebarOpen.set(false);
    this.persistSidebarState();
  }

  onNavigateFromSidebar(): void {
    this.closeSidebar();
  }

  @HostListener('window:keydown.escape')
  onEscape(): void {
    this.closeSidebar();
  }

  private restoreSidebarState(): void {
    const raw = localStorage.getItem(this.sidebarStateKey);
    if (raw === null) {
      this.sidebarOpen.set(false);
      return;
    }

    this.sidebarOpen.set(raw === 'true');
  }

  private persistSidebarState(): void {
    localStorage.setItem(this.sidebarStateKey, String(this.sidebarOpen()));
  }
}
