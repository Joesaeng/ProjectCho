using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Summon : UI_Base
{
    enum Objects
    {
        Particle_Button,
        Particle_Click,
        Particle_Aura,
        Obj_SummonedEquips,
        Button_Summon
    }

    enum SummonState
    {
        Idle,
        OnSummon,
        QuitSummon
    }

    List<Image> _summonItemIcons = new();
    List<Sprite> _summonItemsSprites = new();
    SummonState _summonState = SummonState.Idle;

    public override void Init()
    {
        Bind<GameObject>(typeof(Objects));

        GetObject((int)Objects.Button_Summon).AddUIEvent(Clicked);

        _summonState = SummonState.Idle;
        gameObject.SetActive(false);
    }

    public void OnSummonUI(List<Item> summonItems)
    {
        _summonState = SummonState.Idle;
        Transform summonItemsTf;
        summonItemsTf = GetObject((int)Objects.Obj_SummonedEquips).transform;
        GetObject((int)Objects.Button_Summon).GetComponent<Button>().interactable = false;

        GetObject((int)Objects.Particle_Button).gameObject.SetActive(false);
        GetObject((int)Objects.Particle_Click).gameObject.SetActive(false);
        GetObject((int)Objects.Particle_Aura).gameObject.SetActive(false);

        for (int i = 0; i < summonItemsTf.childCount; i++)
        {
            _summonItemIcons.Add(summonItemsTf.GetChild(i).GetComponent<Image>());
            _summonItemIcons[i].gameObject.SetActive(false);
        }
        foreach (Item item in summonItems)
        {
            _summonItemsSprites.Add(item.ItemIcon);
        }
        gameObject.SetActive(true);
        GetObject((int)Objects.Particle_Button).gameObject.SetActive(true);
        StartCoroutine(CoSummonState_Idle());
    }

    void Clicked(PointerEventData data)
    {
        switch (_summonState)
        {
            case SummonState.Idle:
                GetObject((int)Objects.Button_Summon).GetComponent<Button>().interactable = false;
                _summonState = SummonState.OnSummon;
                StartCoroutine(ConSummonState_OnSummon());
                break;
            case SummonState.OnSummon:
                break;
            case SummonState.QuitSummon:
                gameObject.SetActive(false);
                break;
        }
    }

    IEnumerator CoSummonState_Idle()
    {
        yield return YieldCache.WaitForSeconds(0.1f);
        GetObject((int)Objects.Button_Summon).GetComponent<Button>().interactable = true;
    }

    IEnumerator ConSummonState_OnSummon()
    {
        GetObject((int)Objects.Particle_Button).gameObject.SetActive(false);
        GetObject((int)Objects.Particle_Click).gameObject.SetActive(true);
        GetObject((int)Objects.Particle_Aura).gameObject.SetActive(true);
        yield return YieldCache.WaitForSeconds(0.1f);
        while(_summonItemsSprites.Count > 0)
        {
            int randIconIndex = Random.Range(0,_summonItemIcons.Count);
            int randSpriteIndex = Random.Range(0,_summonItemsSprites.Count);
            _summonItemIcons[randIconIndex].sprite = _summonItemsSprites[randSpriteIndex];
            _summonItemIcons[randIconIndex].gameObject.SetActive(true);

            _summonItemIcons.RemoveAt(randIconIndex);
            _summonItemsSprites.RemoveAt(randSpriteIndex);
            yield return YieldCache.WaitForSeconds(0.08f);
        }
        GetObject((int)Objects.Button_Summon).GetComponent<Button>().interactable = true;
        _summonState = SummonState.QuitSummon;
    }
}
