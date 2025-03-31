using System;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using System.Runtime.ExceptionServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using GoogleMobileAds.Api;


public class Rocket : MonoBehaviour
{
    private int continueLevel;
    //  private BannerView bannerView;
    private InterstitialAd interstitial;
    private RewardedAd rewardedAd;

    public Animator transition;
    Rigidbody rigidbody;
    AudioSource audioSource;
    [SerializeField] float rcsThurst = 50f;
    [SerializeField] float mainThrust = 50f;
    [SerializeField] AudioClip mainEngine;
    [SerializeField] AudioClip Dead;
    [SerializeField] AudioClip nextLevel;
    [SerializeField] float levelLoadDelay = 2f;

    [SerializeReference] bl_Joystick Joystick;
    public int nextSceneLoad;

    [SerializeField]
    private float fuel = 100f;

    [SerializeField]
    private float fuelBurnRate = 20f;
    [SerializeField]
    private float fuelRefillRate = 20f;
    [SerializeField]
    private float refillCoolDown = 2f;
    [SerializeField]
    private Slider fuelSlider;

    private float currentFuel;
    private bool haveFuel = true;
    private float timer = 0;

    [SerializeField] ParticleSystem mainEngineparticle;
    [SerializeField] ParticleSystem Deadparticle;
    [SerializeField] ParticleSystem nextLevelparticle;
    enum State { Alive, Dying, Transcending }
    State state = State.Alive;
    // Start is called before the first frame update
    [SerializeField] public Button thrustbutton;
    private MyButton press;
    public GameObject menuContainer;
    public int count;
    public bool isMenuShown;
    private string appId = "ca-app-pub-4745350216460644~9145482841";
    void Start()
    {

        isMenuShown = false;
        nextSceneLoad = SceneManager.GetActiveScene().buildIndex + 1;
        press = thrustbutton.GetComponent<MyButton>();
        rigidbody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();

        currentFuel = fuel;
        

        MobileAds.Initialize(appId);
        RequestRewardBasedVideo();

    }



    public void RequestInterstitial()
    {
#if UNITY_ANDROID
        string adUnitId = "ca-app-pub-4745350216460644/6158542645";
#elif UNITY_IPHONE
         string adUnitId = "ca-app-pub-3940256099942544/4411468910";
#else
         string adUnitId = "unexpected_platform";
#endif

        // Initialize an InterstitialAd.
        interstitial = new InterstitialAd(adUnitId);
        AdRequest request = new AdRequest.Builder().Build();
        // Load the interstitial with the request.
        interstitial.LoadAd(request);
        interstitial.OnAdLoaded += Interstitial_OnAdLoaded;
    }

    private void Interstitial_OnAdLoaded(object sender, System.EventArgs e)
    {
        interstitial.Show();

        SceneManager.LoadScene(1);
    }

    private void RequestRewardBasedVideo()
    {

#if UNITY_ANDROID
        string adUnitId = "ca-app-pub-4745350216460644/8570767778";
#elif UNITY_IPHONE
            string adUnitId = "ca-app-pub-3940256099942544/1712485313";
#else
        string adUnitId = "unexpected_platform";
#endif
        RewardBasedVideoAd rewardBasedVideo = RewardBasedVideoAd.Instance;

        AdRequest request = new AdRequest.Builder().Build();
        rewardBasedVideo.LoadAd(request, adUnitId);

        //Show Ad
        showAdd(rewardBasedVideo);
    }

    private void showAdd(RewardBasedVideoAd rewardBasedVideo)
    {
        if (rewardBasedVideo.IsLoaded())
        {
            //Subscribe to Ad event
            rewardBasedVideo.OnAdRewarded += HandleRewardBasedVideoRewarded;
            rewardBasedVideo.Show();
        }
    }

    void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            PlayerPrefs.DeleteAll();
        }
        if (state == State.Alive)
        {


            Thurst();

            Rotate();

            fuelSlider.value = currentFuel / fuel;
            if (currentFuel < 0)
            {
                haveFuel = false;
                currentFuel = 0;
                StartDeathSequence();
            }
            if (!haveFuel)
            {
                timer += Time.deltaTime;
                if (timer >= refillCoolDown)
                {
                    haveFuel = true;
                    timer = 0;
                }
            }
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (state != State.Alive)
        {
            return;
        }
        switch (collision.gameObject.tag)
        {
            case "Friendly":
                //nothing
                break;
            case "Finish":
                StartSucessSequence();
                break;
            default:
                count = PlayerPrefs.GetInt("NumOfAdsDeaths");
                count += 1;
                PlayerPrefs.SetInt("NumOfAdsDeaths", count);
                PlayerPrefs.Save();
                StartDeathSequence();
                break;
        }

    }

    private void StartSucessSequence()
    {

        state = State.Transcending;
        nextLevelparticle.Play();

        transition.SetTrigger("Start");

        Invoke("LoadNextScene", levelLoadDelay);
        if (nextSceneLoad > PlayerPrefs.GetInt("levelAt"))
        {
            PlayerPrefs.SetInt("levelAt", nextSceneLoad);
        }
        audioSource.Stop();
        audioSource.PlayOneShot(nextLevel);
        // RequestInterstitial();
    }


    private void StartDeathSequence()
    {
        state = State.Dying;
        audioSource.Stop();
        audioSource.PlayOneShot(Dead);
        Deadparticle.Play();

        if (PlayerPrefs.GetInt("NumOfAdsDeaths") % 3 == 0)
        {

            

            menuContainer.SetActive(true);
            isMenuShown = true;
        }
        else
        {
            Invoke("LoadCurrentScene", levelLoadDelay);
        }
    }

    public void adButton()
    {
        // ad
        Debug.Log("AD");
        // CreateAndLoadRewardedAd();
        // CreateAndLoadRewardedAd();
        RewardBasedVideoAd rewardBasedVideo = RewardBasedVideoAd.Instance;
        showAdd(rewardBasedVideo);
        Invoke("LoadCurrentScene", levelLoadDelay);

    }
    public void HandleRewardBasedVideoRewarded(object sender, Reward args)
    {
        string type = args.Type;
        double amount = args.Amount;
        //Reawrd User here

    }
    public void levelsButton()
    {
        // ad
        //  Debug.Log("AD");
        // CreateAndLoadRewardedAd();
        RequestInterstitial();

    }


    public void exitButton()
    {

        Invoke("LoadMainMenuScene", 0f);

    }

    private void LoadMainMenuScene()
    {

        SceneManager.LoadScene(0);
    }

    private void LoadCurrentScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }

    private void LoadFirstLevel()
    {
        SceneManager.LoadScene(0);
    }

    private void LoadNextScene()
    {

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;
        if (nextSceneIndex == SceneManager.sceneCountInBuildSettings)
        {
            nextSceneIndex = 1;
        }

        SceneManager.LoadScene(nextSceneIndex);
    }

    private void Rotate()
    {
        rigidbody.freezeRotation = true;
        float rotationThisFrame = rcsThurst * Time.deltaTime;
        if (Joystick.Horizontal < 0)
        {
            transform.Rotate(Vector3.forward * rotationThisFrame);
        }
        if (Joystick.Horizontal > 0)
        {
            transform.Rotate(-Vector3.forward * rotationThisFrame);
        }
        rigidbody.freezeRotation = false;

    }

    private void Thurst()
    {

        if (press.buttonPressed && haveFuel)
        {

            rigidbody.AddRelativeForce(Vector3.up * mainThrust * Time.deltaTime);
            currentFuel -= fuelBurnRate * Time.deltaTime;
            if (!audioSource.isPlaying)
            {
                audioSource.PlayOneShot(mainEngine);
                mainEngineparticle.Play();
            }
        }
        else
        {
            audioSource.Stop();
            mainEngineparticle.Stop();
        }
    }

}