using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NumberOfRewards : MonoBehaviour
{

    [SerializeField] private Text _numberOfRewardsText;

    private int _numberOfRewards = 0;

    private void Start()
    {
        AdmobManager.Instance.OnGetReward += SumReward;

        _numberOfRewardsText.text = "Number of rewards = " + _numberOfRewards;
    }

    private void OnDestroy()
    {
        AdmobManager.Instance.OnGetReward -= SumReward;

    }

    private void SumReward()
    {
        _numberOfRewards++;
        _numberOfRewardsText.text = "Number of rewards = " + _numberOfRewards;
    }
}
