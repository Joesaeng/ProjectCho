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
        Obj_SummonedSpells,
        Button_Summon
    }

    enum SummonEquipmentState
    {
        Off,
        Idle,
        OnSummon,
        QuitSummon
    }

    enum SummonSpellState
    {
        Off,
        Idle,
        OnSummon,
        QuitSummon
    }

    List<Image> _summonItemIcons = new();
    List<UI_SummonedSpellIcon> _summonSpellIcons = new();
    List<Sprite> _summonItemsSprites = new();
    Dictionary<int,int> _summonSpellDict = new();

    SummonEquipmentState _summonEquipmentState = SummonEquipmentState.Off;
    SummonSpellState _summonSpellState = SummonSpellState.Off;

    public override void Init()
    {
        Bind<GameObject>(typeof(Objects));

        GetObject((int)Objects.Button_Summon).AddUIEvent(Clicked);

        _summonEquipmentState = SummonEquipmentState.Off;
        _summonSpellState = SummonSpellState.Off;
        gameObject.SetActive(false);
    }

    public void OnSummonSpells(Dictionary<int, int> summonSpells)
    {
        Managers.Sound.Play("ui_summon");
        _summonEquipmentState = SummonEquipmentState.Off;
        GetObject((int)Objects.Obj_SummonedEquips).gameObject.SetActive(false);

        _summonSpellState = SummonSpellState.Idle;
        _summonSpellDict = summonSpells;

        Transform summonSpellsTf = GetObject((int)Objects.Obj_SummonedSpells).transform;
        summonSpellsTf.gameObject.SetActive(true);

        GetObject((int)Objects.Button_Summon).GetComponent<Image>().raycastTarget = false;

        GetObject((int)Objects.Particle_Button).gameObject.SetActive(false);
        GetObject((int)Objects.Particle_Click).gameObject.SetActive(false);
        GetObject((int)Objects.Particle_Aura).gameObject.SetActive(false);

        while(summonSpells.Count > summonSpellsTf.childCount)
        {
            UI_SummonedSpellIcon newIcon = Managers.UI.MakeSubItem<UI_SummonedSpellIcon>(summonSpellsTf);
            newIcon.transform.localPosition = Vector3.zero;
            newIcon.transform.localScale = Vector3.one;
            _summonSpellIcons.Add(newIcon);
        }
        
        for(int i = 0; i < _summonSpellIcons.Count; i++)
        {
            _summonSpellIcons[i].Init();
            _summonSpellIcons[i].gameObject.SetActive(false);
        }

        gameObject.SetActive(true);
        GetObject((int)Objects.Particle_Button).gameObject.SetActive(true);
        StartCoroutine(CoSummonSpellState_Idle());
    }

    public void OnSummonEquips(List<Equipment> summonItems)
    {
        Managers.Sound.Play("ui_summon");
        _summonSpellState = SummonSpellState.Off;
        GetObject((int)Objects.Obj_SummonedSpells).SetActive(false);

        _summonEquipmentState = SummonEquipmentState.Idle;

        Transform summonItemsTf = GetObject((int)Objects.Obj_SummonedEquips).transform;
        summonItemsTf.gameObject.SetActive(true);

        GetObject((int)Objects.Button_Summon).GetComponent<Image>().raycastTarget = false;

        GetObject((int)Objects.Particle_Button).gameObject.SetActive(false);
        GetObject((int)Objects.Particle_Click).gameObject.SetActive(false);
        GetObject((int)Objects.Particle_Aura).gameObject.SetActive(false);

        for (int i = 0; i < summonItemsTf.childCount; i++)
        {
            _summonItemIcons.Add(summonItemsTf.GetChild(i).GetComponent<Image>());
            _summonItemIcons[i].gameObject.SetActive(false);
        }
        foreach (Equipment item in summonItems)
        {
            _summonItemsSprites.Add(item.ItemIcon);
        }
        gameObject.SetActive(true);
        GetObject((int)Objects.Particle_Button).gameObject.SetActive(true);
        StartCoroutine(CoSummonEquipmentState_Idle());
    }

    void Clicked(PointerEventData data)
    {
        if(_summonEquipmentState == SummonEquipmentState.Off)
        {
            switch (_summonSpellState)
            {
                case SummonSpellState.Idle:
                    GetObject((int)Objects.Button_Summon).GetComponent<Image>().raycastTarget = false;
                    _summonSpellState = SummonSpellState.OnSummon;
                    StartCoroutine(CoSummonSpellState_OnSummon());
                    break;
                case SummonSpellState.OnSummon:
                    break;
                case SummonSpellState.QuitSummon:
                    gameObject.SetActive(false);
                    break;
            }
        }
        else
        {
            switch (_summonEquipmentState)
            {
                case SummonEquipmentState.Idle:
                    GetObject((int)Objects.Button_Summon).GetComponent<Image>().raycastTarget = false;
                    _summonEquipmentState = SummonEquipmentState.OnSummon;
                    StartCoroutine(CoSummonEquipmentState_OnSummon());
                    break;
                case SummonEquipmentState.OnSummon:
                    break;
                case SummonEquipmentState.QuitSummon:
                    gameObject.SetActive(false);
                    break;
            }
        }
    }

    IEnumerator CoSummonSpellState_Idle()
    {
        yield return YieldCache.WaitForSeconds(0.1f);
        GetObject((int)Objects.Button_Summon).GetComponent<Image>().raycastTarget = true;
    }

    IEnumerator CoSummonSpellState_OnSummon()
    {
        GetObject((int)Objects.Particle_Button).gameObject.SetActive(false);
        GetObject((int)Objects.Particle_Click).gameObject.SetActive(true);
        int spellIconIndex = 0;
        yield return YieldCache.WaitForSeconds(0.1f);

        foreach(var spellKvp in _summonSpellDict)
        {
            _summonSpellIcons[spellIconIndex].gameObject.SetActive(true);
            _summonSpellIcons[spellIconIndex].SetSummonSpellIcon(spellKvp.Key,spellKvp.Value);
            spellIconIndex++;
            yield return YieldCache.WaitForSeconds(0.15f);
        }

        GetObject((int)Objects.Button_Summon).GetComponent<Image>().raycastTarget = true;
        _summonEquipmentState = SummonEquipmentState.QuitSummon;
    }

    IEnumerator CoSummonEquipmentState_Idle()
    {
        yield return YieldCache.WaitForSeconds(0.1f);
        GetObject((int)Objects.Button_Summon).GetComponent<Image>().raycastTarget = true;
    }

    IEnumerator CoSummonEquipmentState_OnSummon()
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
        GetObject((int)Objects.Button_Summon).GetComponent<Image>().raycastTarget = true;
        _summonEquipmentState = SummonEquipmentState.QuitSummon;
    }


}
