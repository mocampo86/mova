import { Link as RouterLink } from 'react-router-dom';
import { AppBar, Box, Button, IconButton, Toolbar, Typography, useMediaQuery, useTheme } from '@mui/material';
import { useTranslation } from 'react-i18next';
import LanguageSelector from '../components/LanguageSelector';
import { useAuth } from '../features/auth/useAuth';

interface AppHeaderProps {
  greetingName?: string;
  showMenuToggle?: boolean;
  onMenuToggle?: () => void;
}

export default function AppHeader({ greetingName, showMenuToggle, onMenuToggle }: AppHeaderProps) {
  const { t } = useTranslation();
  const { isAuthenticated, user, logout } = useAuth();
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));
  const displayName = greetingName || user?.fullName || user?.email || '';

  return (
    <AppBar
      position="fixed"
      sx={{ zIndex: (theme) => theme.zIndex.drawer + 1 }}
    >
      <Toolbar>
        {showMenuToggle && onMenuToggle && (
          <IconButton
            color="inherit"
            aria-label={t('nav.openMenu')}
            edge="start"
            onClick={onMenuToggle}
            sx={{ mr: 2 }}
          >
            <Typography component="span" sx={{ fontSize: '1.5rem', lineHeight: 1 }}>
              ☰
            </Typography>
          </IconButton>
        )}
        <Typography
          variant="h6"
          noWrap
          component={RouterLink}
          to="/"
          sx={{ color: 'inherit', textDecoration: 'none' }}
        >
          {t('common.brandTitle')}
        </Typography>
        <Box sx={{ flexGrow: 1 }} />
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          {isAuthenticated && user && (
            <>
              {!isMobile && displayName && (
                <Typography sx={{ mr: 1 }}>
                  {t('dashboard.welcome', { name: displayName })}
                </Typography>
              )}
              <LanguageSelector />
              <Button
                color="inherit"
                onClick={logout}
                size="small"
              >
                {t('dashboard.logout')}
              </Button>
            </>
          )}
          {!isAuthenticated && (
            <>
              <Button
                color="inherit"
                component={RouterLink}
                to="/login?intent=user"
                size="small"
              >
                {t('login.userButton')}
              </Button>
              <Button
                color="inherit"
                component={RouterLink}
                to="/login?intent=complex"
                size="small"
              >
                {t('login.adminButton')}
              </Button>
              <LanguageSelector />
            </>
          )}
        </Box>
      </Toolbar>
    </AppBar>
  );
}
