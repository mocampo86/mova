import { getLanguageFlag } from './index';

interface FlagIconProps {
  code: string;
}

export default function FlagIcon({ code }: FlagIconProps) {
  return <span aria-hidden="true">{getLanguageFlag(code) || code.toUpperCase()}</span>;
}
