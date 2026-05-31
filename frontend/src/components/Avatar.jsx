import { getAvatar } from './avatarOptions.js';

export default function Avatar({ avatarKey, size = 'md' }) {
  const avatar = getAvatar(avatarKey);
  return (
    <span className={`avatar avatar-${size}`} title={avatar.label}>
      <img
        alt=""
        src={avatar.image}
        style={{ objectPosition: avatar.position }}
      />
    </span>
  );
}
