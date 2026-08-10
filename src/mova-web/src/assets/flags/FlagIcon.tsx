import { getLanguageFlag } from './index';

interface FlagIconProps {
  code: string;
}

export default function FlagIcon({ code }: FlagIconProps) {
  const flagUrl = getLanguageFlag(code);

  if (!flagUrl) {
    return <span aria-hidden="true">{code.toUpperCase()}</span>;
  }

  return (
    <img
      src={flagUrl}
      alt=""
      aria-hidden="true"
      style={{ width: '1.25em', height: '1.25em', verticalAlign: 'middle' }}
    />
  );
}
