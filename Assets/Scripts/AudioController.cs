using System.Collections;
using System.Collections.Generic;
using Hellmade.Sound;
using NaughtyAttributes;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    private static AudioController _instance;
    public static  AudioController Instance => _instance;

    [BoxGroup("Music")]
    public AudioClip mainMusic;

    [BoxGroup("Sound Effects")]
    public AudioClip villageGoldPerTurnSfx,
                     gettingHitSfx,
                     arrowShootSfx,
                     arrowHitSfx,
                     magicHealSfx,
                     blueKingMagicAttackSfx,
                     darkKingMagicAttackSfx,
                     toggleBarracksSfx,
                     villageUpgradeSfx,
                     poofSfx,
                     endTurnSfx,
                     winSfx,
                     loseSfx;

    [BoxGroup("Good Forces Sound Effects")]
    public AudioClip blueKingSfx, blueSoldierSfx, blueArcherSfx, blueScoutSfx, blueSettlementSfx;
    
    [BoxGroup("Good Forces Attack Sound Effects")]
    public AudioClip blueKingAttackSfx, blueSoldierAttackSfx, blueScoutAttackSfx;

    [BoxGroup("Evil Forces Sound Effects")]
    public AudioClip darkKingSfx, darkSoldierSfx, darkArcherSfx, darkScoutSfx, darkSettlementSfx;
    
    [BoxGroup("Evil Forces Attack Sound Effects")]
    public AudioClip darkKingAttackSfx, darkSoldierAttackSfx, darkScoutAttackSfx;

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(gameObject);

        EazySoundManager.PlayMusic(mainMusic, .7f, true, true, 1f, 1f);
    }
}