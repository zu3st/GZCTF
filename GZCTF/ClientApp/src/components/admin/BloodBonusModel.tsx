import { FC, useEffect, useState } from 'react'
import { useParams } from 'react-router-dom'
import { Button, Group, Modal, ModalProps, NumberInput, Stack, Text } from '@mantine/core'
import { BloodBonus } from '@Utils/ChallengeItem'
import api, { SubmissionType } from '@Api'

const BloodBonusModel: FC<ModalProps> = (props) => {
  const { id } = useParams()
  const numId = parseInt(id ?? '-1')
  const { data: gameSource, mutate } = api.edit.useEditGetGame(numId, {
    refreshInterval: 0,
    revalidateIfStale: false,
    revalidateOnFocus: false,
  })
  const [disabled, setDisabled] = useState(false)
  const [firstBloodBonus, setFirstBloodBonus] = useState(0)
  const [secondBloodBonus, setSecondBloodBonus] = useState(0)
  const [thirdBloodBonus, setThirdBloodBonus] = useState(0)

  useEffect(() => {
    if (gameSource) {
      const bonus = new BloodBonus(gameSource.bloodBonus)
      setFirstBloodBonus(bonus.getBonusNum(SubmissionType.FirstBlood))
      setSecondBloodBonus(bonus.getBonusNum(SubmissionType.SecondBlood))
      setThirdBloodBonus(bonus.getBonusNum(SubmissionType.ThirdBlood))
    }
  }, [gameSource])

  const onUpdate = () => {
    if (gameSource && gameSource.title) {
      setDisabled(true)
      api.edit
        .editUpdateGame(numId, {
          ...gameSource,
          bloodBonus: BloodBonus.fromBonus(firstBloodBonus, secondBloodBonus, thirdBloodBonus)
            .value,
        })
        .then(() => {
          mutate()
          props.onClose()
        })
        .finally(() => {
          setDisabled(false)
        })
    }
  }

  return (
    <Modal {...props}>
      <Stack>
        <Text>
          The first blood bonus is an extra score awarded to the first three teams who solve a challenge, based on the challenge's current score, as a fixed percentage added to the team's score.
        </Text>
        <NumberInput
          label="First blood bonus (%)"
          defaultValue={5}
          precision={1}
          min={0}
          step={1}
          max={100}
          disabled={disabled}
          value={firstBloodBonus / 10}
          onChange={(value) => value && setFirstBloodBonus(Math.floor(value * 10))}
        />
        <NumberInput
          label="Second blood bonus (%)"
          defaultValue={3}
          precision={1}
          min={0}
          step={1}
          max={100}
          disabled={disabled}
          value={secondBloodBonus / 10}
          onChange={(value) => value && setSecondBloodBonus(Math.floor(value * 10))}
        />
        <NumberInput
          label="Third blood bonus (%)"
          defaultValue={1}
          precision={1}
          min={0}
          step={1}
          max={100}
          disabled={disabled}
          value={thirdBloodBonus / 10}
          onChange={(value) => value && setThirdBloodBonus(Math.floor(value * 10))}
        />
        <Group grow style={{ margin: 'auto', width: '100%' }}>
          <Button fullWidth disabled={disabled} onClick={onUpdate}>
            Save changes
          </Button>
        </Group>
      </Stack>
    </Modal>
  )
}

export default BloodBonusModel
