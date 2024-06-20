using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_SpawnMonsterItem : UI_Base
{
    enum Objects
    {
        Tf_SpawnEnemy,
        Text_MonsterElemental
    }

    Transform _tfEnemyPrefab;
    TextMeshProUGUI _monsterElemental;
    public override void Init()
    {
        Bind<GameObject>(typeof(Objects));

        _tfEnemyPrefab = GetObject((int)Objects.Tf_SpawnEnemy).transform;
        _monsterElemental = GetObject((int)Objects.Text_MonsterElemental).GetComponent<TextMeshProUGUI>();
    }

    public void SetSpawnMonster(int enemyId)
    {
        if(_tfEnemyPrefab.TryGetChild(0,out Transform child))
        {
            Managers.Resource.Destroy(child.gameObject);
        }
        _monsterElemental.text = Language.GetLanguage(Managers.Data.BaseEnemyDataDict[enemyId].elementType.ToString());
        _monsterElemental.color = ConstantData.TextColorsByElementTypes[(int)Managers.Data.BaseEnemyDataDict[enemyId].elementType];

        GameObject obj = Resources.Load<GameObject>("Prefabs/Enemys/" + Managers.Data.BaseEnemyDataDict[enemyId].prefabName);
        Instantiate(obj, _tfEnemyPrefab);
        obj.transform.localScale = Vector3.one;
    }
}
