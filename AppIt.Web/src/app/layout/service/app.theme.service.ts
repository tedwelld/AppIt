import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class AppThemeService {
    setTopbarColor(color: string): void {
        document.documentElement.style.setProperty('--app-topbar-color', color);
    }

    setColorScheme(scheme: 'light' | 'dark'): void {
        const themeLink = document.getElementById('theme-css') as HTMLLinkElement | null;
        if (!themeLink) {
            return;
        }

        themeLink.href =
            scheme === 'dark'
                ? 'assets/layout/styles/theme/md-dark-indigo/theme.css'
                : 'assets/layout/styles/theme/md-light-indigo/theme.css';
    }

    refreshTheme(): void {
        this.setTopbarColor(getComputedStyle(document.documentElement).getPropertyValue('--app-topbar-color').trim() || '#4CAF50');
    }
}
