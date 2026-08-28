import { Box, Typography } from '@mui/material';
import { useSearchParams, Link as RouterLink } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import Seo from '../components/Seo';
import { GoogleLoginButton } from '../features/auth/GoogleLoginButton';

export default function LoginPage() {
  const { t } = useTranslation();
  const [searchParams] = useSearchParams();
  const intent = searchParams.get('intent') === 'complex' ? 'complex' : 'user';

  const isComplex = intent === 'complex';

  return (
    <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 3, mt: 8, px: 2 }}>
      <Seo
        title={`${t('common.appName')} | ${isComplex ? t('seo.loginComplexTitle') : t('seo.loginUserTitle')}`}
        description={isComplex ? t('seo.loginComplexDescription') : t('seo.loginUserDescription')}
      />
      <Typography variant="h4">{isComplex ? t('login.complexTitle') : t('login.userTitle')}</Typography>
      <Typography variant="body1" textAlign="center">
        {isComplex ? t('login.complexSubtitle') : t('login.userSubtitle')}
      </Typography>
      <GoogleLoginButton intent={intent} />
      <Typography variant="body2" color="text.secondary">
        {isComplex ? (
          <>{t('login.switchToUser')} <RouterLink to="/login?intent=user">{t('login.switchToUserLink')}</RouterLink></>
        ) : (
          <>{t('login.switchToComplex')} <RouterLink to="/login?intent=complex">{t('login.switchToComplexLink')}</RouterLink></>
        )}
      </Typography>
    </Box>
  );
}
