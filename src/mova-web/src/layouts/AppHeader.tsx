import type { ReactNode } from 'react';
import { useCallback, useState } from 'react';
import { Link as RouterLink, useLocation } from 'react-router-dom';
import {
  AppBar,
  Box,
  Button,
  Divider,
  Drawer,
  IconButton,
  List,
  ListItem,
  ListItemButton,
  ListItemText,
  Toolbar,
  Typography,
  useMediaQuery,
  useTheme
} from '@mui/material';
import { useTranslation } from 'react-i18next';
import LanguageSelector from '../components/LanguageSelector';
import { useAuth } from '../features/auth/useAuth';

interface AppHeaderProps {
  greetingName?: string;
  showMenuToggle?: boolean;
  onMenuToggle?: () => void;
}

function isActivePath(pathname: string, to: string, exact = false): boolean {
  const normalized = to.endsWith('/') ? to.slice(0, -1) : to;
  if (pathname === normalized) return true;
  if (exact) return false;
  return pathname.startsWith(`${normalized}/`);
}

function isLoginIntentActive(pathname: string, search: string, intent: string): boolean {
  if (pathname !== '/login') return false;
  const params = new URLSearchParams(search);
  return params.get('intent') === intent;
}

interface MobileNavLinkProps {
  to: string;
  active?: boolean;
  onClick?: () => void;
  children: ReactNode;
}

function MobileNavLink({ to, active, onClick, children }: MobileNavLinkProps) {
  return (
    <ListItem disablePadding>
      <ListItemButton
        component={RouterLink}
        to={to}
        selected={active}
        aria-current={active ? 'page' : undefined}
        onClick={onClick}
      >
        <ListItemText primary={children} />
      </ListItemButton>
    </ListItem>
  );
}

const focusVisibleSx = {
  '&:focus-visible': {
    outline: '2px solid currentColor',
    outlineOffset: '2px'
  }
};

const mobileMenuId = 'app-header-mobile-menu';

export default function AppHeader({ greetingName, showMenuToggle, onMenuToggle }: AppHeaderProps) {
  const { t } = useTranslation();
  const { pathname, search } = useLocation();
  const { isAuthenticated, user, logout } = useAuth();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));
  const isExtraSmall = useMediaQuery(theme.breakpoints.down('sm'));
  const displayName = greetingName || user?.fullName || user?.email || '';
  const [mobileNavOpen, setMobileNavOpen] = useState(false);

  const openMobileNav = useCallback(() => setMobileNavOpen(true), []);
  const closeMobileNav = useCallback(() => setMobileNavOpen(false), []);

  const isHomeActive = pathname === '/';
  const isDashboardActive = isActivePath(pathname, '/user');
  const showDashboardLink = isAuthenticated && pathname !== '/user';

  const handleLogout = useCallback(() => {
    closeMobileNav();
    logout();
  }, [closeMobileNav, logout]);

  const publicMenu = (
    <>
      <Toolbar>
        <Typography
          variant="h6"
          component={RouterLink}
          to="/"
          onClick={closeMobileNav}
          sx={{ color: 'inherit', textDecoration: 'none', ...focusVisibleSx }}
        >
          {t('nav.brand')}
        </Typography>
      </Toolbar>
      <Divider />
      <List>
        <MobileNavLink
          to="/login?intent=user"
          active={isLoginIntentActive(pathname, search, 'user')}
          onClick={closeMobileNav}
        >
          {t('login.userButton')}
        </MobileNavLink>
        <MobileNavLink
          to="/login?intent=complex"
          active={isLoginIntentActive(pathname, search, 'complex')}
          onClick={closeMobileNav}
        >
          {t('login.adminButton')}
        </MobileNavLink>
        <ListItem>
          <LanguageSelector />
        </ListItem>
      </List>
    </>
  );

  const accountMenu = (
    <>
      <Toolbar>
        <Typography variant="h6" component="div">
          {t('nav.brand')}
        </Typography>
      </Toolbar>
      <Divider />
      <List>
        {showDashboardLink && (
          <MobileNavLink to="/user" active={isDashboardActive} onClick={closeMobileNav}>
            {t('nav.dashboard')}
          </MobileNavLink>
        )}
        <ListItem>
          <LanguageSelector />
        </ListItem>
        <ListItem disablePadding>
          <ListItemButton onClick={handleLogout} aria-label={t('dashboard.logout')}>
            <ListItemText primary={t('dashboard.logout')} />
          </ListItemButton>
        </ListItem>
      </List>
    </>
  );

  const hamburger = (
    <IconButton
      color="inherit"
      aria-label={t('nav.openMenu')}
      aria-expanded={mobileNavOpen}
      aria-controls={mobileMenuId}
      onClick={openMobileNav}
      sx={{ ...focusVisibleSx }}
    >
      <Typography component="span" aria-hidden="true" sx={{ fontSize: '1.5rem', lineHeight: 1 }}>
        {'☰'}
      </Typography>
    </IconButton>
  );

  const mobileNav = (
    <>
      {hamburger}
      <Drawer
        anchor="right"
        open={mobileNavOpen}
        onClose={closeMobileNav}
        ModalProps={{ keepMounted: true }}
        PaperProps={{ id: mobileMenuId, role: 'dialog', 'aria-modal': true, 'aria-label': t('nav.brand') }}
      >
        <Box sx={{ width: 260 }} role="navigation" aria-label={t('nav.brand')}>
          {isAuthenticated ? accountMenu : publicMenu}
        </Box>
      </Drawer>
    </>
  );

  const publicDesktopActions = (
    <>
      <Button
        color="inherit"
        component={RouterLink}
        to="/login?intent=user"
        size="small"
        aria-current={isLoginIntentActive(pathname, search, 'user') ? 'page' : undefined}
        sx={{ ...focusVisibleSx }}
      >
        {t('login.userButton')}
      </Button>
      <Button
        color="inherit"
        component={RouterLink}
        to="/login?intent=complex"
        size="small"
        aria-current={isLoginIntentActive(pathname, search, 'complex') ? 'page' : undefined}
        sx={{ ...focusVisibleSx }}
      >
        {t('login.adminButton')}
      </Button>
      <LanguageSelector />
    </>
  );

  const accountDesktopActions = (
    <>
      {showDashboardLink && (
        <Button
          color="inherit"
          component={RouterLink}
          to="/user"
          size="small"
          aria-current={isDashboardActive ? 'page' : undefined}
          sx={{ ...focusVisibleSx }}
        >
          {t('nav.dashboard')}
        </Button>
      )}
      {!isMobile && displayName && (
        <Typography
          noWrap
          sx={{
            mr: 1,
            maxWidth: { xs: 140, sm: 180, md: 260 },
            overflow: 'hidden',
            textOverflow: 'ellipsis'
          }}
        >
          {t('dashboard.welcome', { name: displayName })}
        </Typography>
      )}
      <LanguageSelector />
      <Button color="inherit" onClick={logout} size="small" sx={{ ...focusVisibleSx }}>
        {t('dashboard.logout')}
      </Button>
    </>
  );

  const authenticatedActions = (() => {
    if (showMenuToggle) {
      return (
        <>
          {showDashboardLink && !isExtraSmall && (
            <Button
              color="inherit"
              component={RouterLink}
              to="/user"
              size="small"
              aria-current={isDashboardActive ? 'page' : undefined}
              sx={{ ...focusVisibleSx }}
            >
              {t('nav.dashboard')}
            </Button>
          )}
          {!isMobile && displayName && (
            <Typography
              noWrap
              sx={{
                mr: 1,
                maxWidth: { xs: 140, sm: 180, md: 260 },
                overflow: 'hidden',
                textOverflow: 'ellipsis'
              }}
            >
              {t('dashboard.welcome', { name: displayName })}
            </Typography>
          )}
          <LanguageSelector />
          <Button color="inherit" onClick={logout} size="small" sx={{ ...focusVisibleSx }}>
            {t('dashboard.logout')}
          </Button>
        </>
      );
    }

    if (isExtraSmall) {
      return mobileNav;
    }

    return accountDesktopActions;
  })();

  return (
    <AppBar position="fixed" sx={{ zIndex: (theme) => theme.zIndex.drawer + 1 }}>
      <Toolbar
        component="nav"
        aria-label={t('nav.brand')}
        sx={{ gap: 1, flexWrap: 'nowrap' }}
      >
        {showMenuToggle && onMenuToggle && (
          <IconButton
            color="inherit"
            aria-label={t('nav.openMenu')}
            edge="start"
            onClick={onMenuToggle}
            sx={{ ...focusVisibleSx }}
          >
            <Typography component="span" aria-hidden="true" sx={{ fontSize: '1.5rem', lineHeight: 1 }}>
              {'☰'}
            </Typography>
          </IconButton>
        )}
        <Typography
          variant="h6"
          noWrap
          component={RouterLink}
          to="/"
          aria-current={isHomeActive ? 'page' : undefined}
          sx={{ color: 'inherit', textDecoration: 'none', ...focusVisibleSx }}
        >
          {t('nav.brand')}
        </Typography>
        <Box sx={{ flexGrow: 1 }} />
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, minWidth: 0 }}>
          {isAuthenticated ? authenticatedActions : isMobile ? mobileNav : publicDesktopActions}
        </Box>
      </Toolbar>
    </AppBar>
  );
}
