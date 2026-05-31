export const avatarOptions = [
  { key: 'mentor', label: 'Formal mentor', image: '/avatars/mentor.svg', position: '50% 44%' },
  { key: 'builder', label: 'Glasses professional', image: '/avatars/builder.svg', position: '50% 42%' },
  { key: 'analyst', label: 'Green sweater', image: '/avatars/analyst.svg', position: '50% 43%' },
  { key: 'designer', label: 'Thumbs up', image: '/avatars/designer.svg', position: '50% 42%' },
  { key: 'architect', label: 'Smiling portrait', image: '/avatars/architect.svg', position: '50% 42%' },
  { key: 'expert', label: 'Glasses portrait', image: '/avatars/expert.svg', position: '50% 42%' }
];

export function getAvatar(key) {
  return avatarOptions.find((avatar) => avatar.key === key) || avatarOptions[0];
}
