import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AvatarModule } from 'primeng/avatar';
import { ButtonModule } from 'primeng/button';
import { MenuModule } from 'primeng/menu';
import { MenuItem } from 'primeng/api';
import { LayoutService } from '../service/layout.service';
import { AuthService } from '../../core/auth/auth.service';

@Component({
    selector: 'app-topbar',
    standalone: true,
    imports: [CommonModule, RouterLink, AvatarModule, ButtonModule, MenuModule],
    template: `
        <header class="layout-topbar">
            <div class="layout-topbar-logo-container">
                <button class="layout-menu-button layout-topbar-action" type="button" (click)="layoutService.onMenuToggle()">
                    <i class="pi pi-bars"></i>
                </button>
                <a class="layout-topbar-logo" routerLink="/">
                    <div class="flex items-center gap-3">
                        <div class="w-11 h-11 rounded-2xl bg-primary text-primary-contrast flex items-center justify-center font-bold text-xl">AI</div>
                        <div class="flex flex-col">
                            <span class="font-display font-bold text-xl leading-none">AppIt</span>
                            <span class="text-xs text-muted-color uppercase tracking-[0.24em]">{{ workspaceLabel() }}</span>
                        </div>
                    </div>
                </a>
            </div>

            <div class="layout-topbar-actions">
                <div class="hidden xl:flex items-center gap-2 px-4 py-2 rounded-full bg-surface-0/60 dark:bg-surface-900/50 border border-surface-200 dark:border-surface-700">
                    <span class="text-xs uppercase tracking-[0.25em] text-muted-color font-semibold">Signed in</span>
                    <span class="text-sm font-semibold">{{ auth.displayName() }}</span>
                </div>

                <button type="button" class="layout-topbar-action" (click)="toggleDarkMode()">
                    <i [ngClass]="{ 'pi ': true, 'pi-moon': layoutService.isDarkTheme(), 'pi-sun': !layoutService.isDarkTheme() }"></i>
                </button>
                <button type="button" class="layout-topbar-action layout-topbar-action-highlight" (click)="goHome()">
                    <i class="pi pi-bolt"></i>
                </button>
                <button pButton type="button" label="Logout" icon="pi pi-sign-out" severity="danger" class="logout-button" (click)="auth.logout()"></button>
                <button type="button" class="layout-topbar-action flex items-center gap-1.5 px-2 rounded-full" (click)="profileMenu.toggle($event)">
                    <p-avatar [label]="avatarLabel()" shape="circle" styleClass="bg-gradient-to-br from-blue-600 to-violet-600 text-white !text-sm !w-8 !h-8"></p-avatar>
                    <i class="pi pi-chevron-down text-xs text-muted-color"></i>
                </button>
                <p-menu #profileMenu [popup]="true" [model]="profileItems" appendTo="body"></p-menu>
            </div>
        </header>
    `
})
export class AppTopbar {
    private readonly router = inject(Router);
    readonly auth = inject(AuthService);
    profileItems: MenuItem[] = [
        { label: 'Dashboard', icon: 'pi pi-fw pi-home', command: () => void this.router.navigateByUrl(this.homeRoute()) },
        { label: 'Settings', icon: 'pi pi-fw pi-cog', command: () => void this.router.navigateByUrl(this.settingsRoute()) },
        { label: 'Logout', icon: 'pi pi-fw pi-sign-out', command: () => this.auth.logout() }
    ];

    constructor(public layoutService: LayoutService) {}

    workspaceLabel(): string {
        return this.auth.isAdmin() ? 'Admin Control Center' : 'Guest Portal';
    }

    avatarLabel(): string {
        return this.auth
            .displayName()
            .split(' ')
            .filter(Boolean)
            .slice(0, 2)
            .map((part) => part[0]?.toUpperCase() ?? '')
            .join('') || 'AI';
    }

    goHome(): void {
        void this.router.navigateByUrl(this.homeRoute());
    }

    homeRoute(): string {
        return this.auth.isAdmin() ? '/admin/dashboard' : '/user/dashboard';
    }

    settingsRoute(): string {
        return this.auth.isAdmin() ? '/admin/settings' : '/user/settings';
    }

    toggleDarkMode(): void {
        this.layoutService.layoutConfig.update((state) => ({ ...state, darkTheme: !state.darkTheme }));
    }
}
