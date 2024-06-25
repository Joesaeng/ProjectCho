using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using Newtonsoft.Json;
using Data;

public class FirebaseManager 
{
    private static FirebaseManager instance = null;
    public static FirebaseManager Instance
    {
        get
        {
            if(instance == null)
            {
                instance = new FirebaseManager();
            }
            return instance;
        }
    }

    private FirebaseAuth auth; // 로그인 / 회원가입 등에 사용
    private FirebaseUser user; // 인증이 완료된 유저 정보

    public string CurrentUserId => user?.UserId;

    private bool isSignIn = false;

    private DatabaseReference databaseReference;

    public void Init()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result != DependencyStatus.Available)
            {
                Debug.LogError($"Could not resolve all Firebase dependencies: {task.Result}");
                return;
            }

            FirebaseApp app = FirebaseApp.DefaultInstance;

            // FirebaseDatabase 인스턴스를 가져와 URL 설정
            FirebaseDatabase database = FirebaseDatabase.GetInstance(app);
            database.SetPersistenceEnabled(false);  // 필요에 따라 설정
            databaseReference = database.RootReference;

            auth = FirebaseAuth.DefaultInstance;
            auth.StateChanged += OnChanged;

            // 초기화 시점에 이미 로그인된 사용자가 있는지 확인
            if (auth.CurrentUser != null)
            {
                user = auth.CurrentUser;
                isSignIn = true;
                Debug.LogFormat("User signed in automatically: {0} ({1})", user.DisplayName, user.UserId);
            }
        });
    }

    public void SignOut()
    {
        if (auth != null)
        {
            auth.SignOut();
            user = null;
            isSignIn = false;
        }
    }

    private void OnChanged(object sender, EventArgs e)
    {
        if(auth.CurrentUser != user)
        {
            isSignIn = auth.CurrentUser != user && auth.CurrentUser != null;
            if(!isSignIn && user != null)
            {
                Debug.LogFormat("Signed out {0}", user.UserId);
            }

            user = auth.CurrentUser;
            if(isSignIn)
            {
                Debug.LogFormat("Signed in {0}", user.UserId);
            }
        }
    }

    public void TryGuestLogin()
    {
        auth.SignInAnonymouslyAsync().ContinueWith(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInAnonymouslyAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
                return;
            }

            user = task.Result.User;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                user.DisplayName, user.UserId);

            // 게스트 ID를 PlayerPrefs에 저장합니다.
            PlayerPrefs.SetString("guestid", user.UserId);
            PlayerPrefs.Save(); // 변경 사항 저장
        });
    }

    public void SavePlayerData(string playerData)
    {
        if (databaseReference == null)
        {
            Debug.LogError("databaseReference is null");
            return;
        }

        if (user == null)
        {
            Debug.LogError("user is null");
            return;
        }

        databaseReference.Child("users").Child(user.UserId).SetRawJsonValueAsync(playerData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SavePlayerData was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SavePlayerData encountered an error: " + task.Exception);
                foreach (var e in task.Exception.Flatten().InnerExceptions)
                {
                    FirebaseException firebaseEx = e as FirebaseException;
                    if (firebaseEx != null)
                    {
                        Debug.LogError("FirebaseException: " + firebaseEx.Message);
                    }
                    else
                    {
                        Debug.LogError("Exception: " + e.Message);
                    }
                }
                return;
            }
        
            Debug.Log("PlayerData saved successfully.");
        });
    }

    public void LoadPlayerData(string userId, System.Action<PlayerData> onLoaded)
    {
        databaseReference.Child("users").Child(userId).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("LoadPlayerData was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("LoadPlayerData encountered an error: " + task.Exception);
                return;
            }

            DataSnapshot snapshot = task.Result;
            if (snapshot.Exists)
            {
                string json = snapshot.GetRawJsonValue();
                //var settings = new JsonSerializerSettings
                //{
                //    Converters = { new ItemDataConverter()},
                //};
                PlayerData playerData = JsonConvert.DeserializeObject<PlayerData>(json);
                onLoaded?.Invoke(playerData);
            }
            else
            {
                Debug.LogWarning($"No data found at path: users/{userId}");
                onLoaded?.Invoke(null); // 또는 새로운 PlayerData 객체를 전달할 수 있습니다.
            }
        });
    }
}
