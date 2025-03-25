using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainManager : MonoBehaviour
{
    #region INSPECTOR PROPERTIES

    public GameManager GameManager;
    public UIManager UIManager;
    public EventManager EventManager;
    public EventRunner EventRunner;

    // public CameraManager CameraManager;
    // public UpgradeManager UpgradeManager;
    // public LevelManager LevelManager;
    // public AudioManager AudioManager;

    #endregion

    #region Singleton

    public static MainManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }

        InitializeAwake();
    }

    #endregion

    #region UNITY METHODS

    private void Start()
    {
        InitializeStart();
    }

    #endregion

    #region PUBLIC METHODS

    private void InitializeAwake()
    {
        // UpgradeManager.Initialize();
        EventManager.Initialize();
        // GameManager.Initialize();
        // LevelManager.Initialize();
    }

    private void InitializeStart()
    {
        // UIManager.Initialize();
    }

    #endregion
}