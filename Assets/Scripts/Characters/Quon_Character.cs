using UnityEngine;

public class Quon_Character : Character
{
    protected readonly AttackData[] quon_attacks = new AttackData[]
    {
        new AttackData
        {
            attackName = "light_attack_1",
            attackDisplayName = "Quon Kick",
            framesActive = 10
        },
        new AttackData
        {
            attackName = "special_attack_1",
            attackDisplayName = "Sawblade Slash",
            framesActive = 30
        }
    };

    public GameObject sawBladePrefab;
    public Transform sawBladeSpawnPoint;

    protected override void Start()
    {
        base.Start();
        SetAttacks(quon_attacks);
    }

    public void SawBlade()
    {
        Instantiate(sawBladePrefab, sawBladeSpawnPoint.position, sawBladeSpawnPoint.rotation);
    }
}

