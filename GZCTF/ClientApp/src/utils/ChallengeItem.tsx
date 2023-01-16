import { forwardRef } from 'react'
import { Group, MantineColor, Stack, Text, useMantineTheme } from '@mantine/core'
import {
  mdiBomb,
  mdiBullhornOutline,
  mdiCellphoneCog,
  mdiChevronTripleLeft,
  mdiChip,
  mdiConsole,
  mdiEthereum,
  mdiFingerprint,
  mdiFlag,
  mdiGamepadVariantOutline,
  mdiHexagonSlice2,
  mdiHexagonSlice4,
  mdiHexagonSlice6,
  mdiLightbulbOnOutline,
  mdiMatrix,
  mdiPlus,
  mdiWeb,
} from '@mdi/js'
import { Icon } from '@mdi/react'
import { ChallengeTag, ChallengeType, NoticeType, SubmissionType } from '@Api'

export const ChallengeTypeLabelMap = new Map<ChallengeType, ChallengeTypeItemProps>([
  [ChallengeType.StaticAttachment, { label: 'Static Attachment', desrc: 'Shared attachment, any flag can be submitted' }],
  [ChallengeType.StaticContainer, { label: 'Static Container', desrc: 'Shared container, any flag can be submitted' }],
  [
    ChallengeType.DynamicAttachment,
    { label: 'Dynamic Attachment', desrc: 'Dynamic Attachment, each team has a unique attachment' },
  ],
  [ChallengeType.DynamicContainer, { label: 'Dynamic Container', desrc: 'Auto-generated flag, unique for each team' }],
])

export interface ChallengeTypeItemProps extends React.ComponentPropsWithoutRef<'div'> {
  label: string
  desrc: string
}

export const ChallengeTypeItem = forwardRef<HTMLDivElement, ChallengeTypeItemProps>(
  ({ label, desrc, ...others }: ChallengeTypeItemProps, ref) => {
    return (
      <Stack spacing={0} ref={ref} {...others}>
        <Text size="sm">{label}</Text>
        <Text size="xs">{desrc}</Text>
      </Stack>
    )
  }
)

export const ChallengeTagLabelMap = new Map<ChallengeTag, ChallengeTagItemProps>([
  [
    ChallengeTag.Misc,
    { desrc: 'Misc', icon: mdiGamepadVariantOutline, label: ChallengeTag.Misc, color: 'teal' },
  ],
  [
    ChallengeTag.Crypto,
    { desrc: 'Crypto', icon: mdiMatrix, label: ChallengeTag.Crypto, color: 'indigo' },
  ],
  [ChallengeTag.Pwn, { desrc: 'Pwn', icon: mdiBomb, label: ChallengeTag.Pwn, color: 'red' }],
  [ChallengeTag.Web, { desrc: 'Web', icon: mdiWeb, label: ChallengeTag.Web, color: 'blue' }],
  [
    ChallengeTag.Reverse,
    { desrc: 'Reverse', icon: mdiChevronTripleLeft, label: ChallengeTag.Reverse, color: 'yellow' },
  ],
  [
    ChallengeTag.Blockchain,
    { desrc: 'Blockchain', icon: mdiEthereum, label: ChallengeTag.Blockchain, color: 'lime' },
  ],
  [
    ChallengeTag.Forensics,
    { desrc: 'Forensics', icon: mdiFingerprint, label: ChallengeTag.Forensics, color: 'cyan' },
  ],
  [
    ChallengeTag.Hardware,
    { desrc: 'Hardware', icon: mdiChip, label: ChallengeTag.Hardware, color: 'violet' },
  ],
  [
    ChallengeTag.Mobile,
    { desrc: 'Mobile', icon: mdiCellphoneCog, label: ChallengeTag.Mobile, color: 'pink' },
  ],
  [ChallengeTag.PPC, { desrc: 'PPC', icon: mdiConsole, label: ChallengeTag.PPC, color: 'orange' }],
])

export interface ChallengeTagItemProps extends React.ComponentPropsWithoutRef<'div'> {
  label: ChallengeTag
  desrc: string
  icon: string
  color: MantineColor
}

export const ChallengeTagItem = forwardRef<HTMLDivElement, ChallengeTagItemProps>(
  ({ label, icon, color, ...others }: ChallengeTagItemProps, ref) => {
    const theme = useMantineTheme()
    return (
      <Group ref={ref} noWrap {...others}>
        <Icon color={theme.colors[color][4]} path={icon} size={1} />
        <Text size="sm">{label}</Text>
      </Group>
    )
  }
)

export const BloodsTypes = [
  SubmissionType.FirstBlood,
  SubmissionType.SecondBlood,
  SubmissionType.ThirdBlood,
]

export const SubmissionTypeColorMap = () => {
  const theme = useMantineTheme()
  return new Map([
    [SubmissionType.Unaccepted, undefined],
    [SubmissionType.Normal, theme.colors.brand[theme.colorScheme === 'dark' ? 8 : 6]],
    [SubmissionType.FirstBlood, theme.colors.yellow[5]],
    [
      SubmissionType.SecondBlood,
      theme.colorScheme === 'dark'
        ? theme.fn.lighten(theme.colors.gray[2], 0.3)
        : theme.fn.darken(theme.colors.gray[1], 0.2),
    ],
    [
      SubmissionType.ThirdBlood,
      theme.colorScheme === 'dark'
        ? theme.fn.darken(theme.colors.orange[7], 0.25)
        : theme.fn.lighten(theme.colors.orange[7], 0.2),
    ],
  ])
}

export const SubmissionTypeIconMap = (size: number) => {
  const colorMap = SubmissionTypeColorMap()
  return {
    iconMap: new Map([
      [SubmissionType.Unaccepted, undefined],
      [
        SubmissionType.Normal,
        <Icon path={mdiFlag} size={size} color={colorMap.get(SubmissionType.Normal)} />,
      ],
      [
        SubmissionType.FirstBlood,
        <Icon
          path={mdiHexagonSlice6}
          size={size}
          color={colorMap.get(SubmissionType.FirstBlood)}
        />,
      ],
      [
        SubmissionType.SecondBlood,
        <Icon
          path={mdiHexagonSlice4}
          size={size}
          color={colorMap.get(SubmissionType.SecondBlood)}
        />,
      ],
      [
        SubmissionType.ThirdBlood,
        <Icon
          path={mdiHexagonSlice2}
          size={size}
          color={colorMap.get(SubmissionType.ThirdBlood)}
        />,
      ],
    ]),
    colorMap,
  }
}

export const NoticTypeIconMap = (size: number) => {
  const theme = useMantineTheme()
  const { iconMap } = SubmissionTypeIconMap(size)
  const colorIdx = theme.colorScheme === 'dark' ? 4 : 7

  return new Map([
    [
      NoticeType.Normal,
      <Icon path={mdiBullhornOutline} size={size} color={theme.colors.brand[colorIdx]} />,
    ],
    [
      NoticeType.NewHint,
      <Icon path={mdiLightbulbOnOutline} size={size} color={theme.colors.yellow[colorIdx]} />,
    ],
    [
      NoticeType.NewChallenge,
      <Icon path={mdiPlus} size={size} color={theme.colors.green[colorIdx]} />,
    ],
    [NoticeType.FirstBlood, iconMap.get(SubmissionType.FirstBlood)],
    [NoticeType.SecondBlood, iconMap.get(SubmissionType.SecondBlood)],
    [NoticeType.ThirdBlood, iconMap.get(SubmissionType.ThirdBlood)],
  ])
}

export interface BonusLabel {
  name: string
  desrc: string
}

const BonusLabelNameMap = new Map([
  [SubmissionType.FirstBlood, 'First Blood'],
  [SubmissionType.SecondBlood, 'Second Blood'],
  [SubmissionType.ThirdBlood, 'Third Blood'],
])

export class BloodBonus {
  private val: number = (50 << 20) + (30 << 10) + 10
  private static mask = 0x3ff
  private static base = 1000

  static default = new BloodBonus()

  constructor(val?: number) {
    this.val = val ?? this.val
  }

  get value() {
    return this.val
  }

  getBonusNum(type: SubmissionType) {
    if (type === SubmissionType.FirstBlood) return (this.val >> 20) & BloodBonus.mask
    if (type === SubmissionType.SecondBlood) return (this.val >> 10) & BloodBonus.mask
    if (type === SubmissionType.ThirdBlood) return this.val & BloodBonus.mask
    return 0
  }

  getBonus(type: SubmissionType) {
    if (type === SubmissionType.Unaccepted) return 0
    if (type === SubmissionType.Normal) return 1

    const num = this.getBonusNum(type)
    if (num === 0) return 0

    return num / BloodBonus.base
  }

  getBonusLabels() {
    return new Map(
      BloodsTypes.map((type) => {
        const bonus = this.getBonusNum(type)
        return [
          type,
          {
            name: BonusLabelNameMap.get(type),
            desrc: `+${bonus / (BloodBonus.base / 100)}%`,
          } as BonusLabel,
        ]
      })
    )
  }

  static fromBonus(first: number, second: number, third: number) {
    const value = (first << 20) + (second << 10) + third
    return new BloodBonus(value)
  }
}
