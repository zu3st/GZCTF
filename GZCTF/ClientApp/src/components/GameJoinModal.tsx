import { FC, useState } from 'react'
import { useParams } from 'react-router-dom'
import { Button, Modal, ModalProps, Select, Stack, TextInput } from '@mantine/core'
import { showNotification } from '@mantine/notifications'
import { mdiClose } from '@mdi/js'
import { Icon } from '@mdi/react'
import { useTeams } from '@Utils/useUser'
import api, { GameJoinModel } from '@Api'

interface GameJoinModalProps extends ModalProps {
  onSubmitJoin: (info: GameJoinModel) => Promise<void>
  currentOrganization?: string | null
}

const GameJoinModal: FC<GameJoinModalProps> = (props) => {
  const { id } = useParams()
  const numId = parseInt(id ?? '-1')
  const { onSubmitJoin, currentOrganization, ...modalProps } = props
  const { teams } = useTeams()

  const { data: game } = api.game.useGameGames(numId, {
    refreshInterval: 0,
    revalidateOnFocus: false,
  })

  const [inviteCode, setInviteCode] = useState('')
  const [organization, setOrganization] = useState(currentOrganization ?? '')
  const [team, setTeam] = useState('')
  const [disabled, setDisabled] = useState(false)

  return (
    <Modal {...modalProps}>
      <Stack>
        <Select
          required
          label="Select your team"
          description="Please select a team to join the game, you won't be able to change it later"
          data={teams?.map((t) => ({ label: t.name!, value: t.id!.toString() })) ?? []}
          disabled={disabled}
          value={team}
          onChange={(e) => setTeam(e ?? '')}
        />
        {game?.inviteCodeRequired && (
          <TextInput
            required
            label="Enter invite code"
            description="This game requires an invite code to join"
            value={inviteCode}
            onChange={(e) => setInviteCode(e.target.value)}
            disabled={disabled}
          />
        )}
        {game?.organizations && game.organizations.length > 0 && (
          <Select
            required
            label="Select your organization"
            description="This game allows multiple organizations to join, please select yours"
            data={game.organizations}
            disabled={disabled}
            value={organization}
            onChange={(e) => setOrganization(e ?? '')}
          />
        )}
        <Button
          disabled={disabled}
          onClick={() => {
            setDisabled(true)

            if (game?.inviteCodeRequired && !inviteCode) {
              showNotification({
                color: 'orange',
                message: 'Invite code is required',
                icon: <Icon path={mdiClose} size={1} />,
                disallowClose: true,
              })
              return
            }

            if (game?.organizations && game.organizations.length > 0 && !organization) {
              showNotification({
                color: 'orange',
                message: 'Organization is required',
                icon: <Icon path={mdiClose} size={1} />,
                disallowClose: true,
              })
              return
            }

            onSubmitJoin({
              teamId: parseInt(team),
              inviteCode: game?.inviteCodeRequired ? inviteCode : undefined,
              organization:
                game?.organizations && game.organizations.length > 0 ? organization : undefined,
            }).finally(() => {
              setInviteCode('')
              setOrganization('')
              setDisabled(false)
              props.onClose()
            })
          }}
        >
          Join
        </Button>
      </Stack>
    </Modal>
  )
}

export default GameJoinModal
