# Openfort In-App Purchase Advanced Sample

<div align="center">
    <img
      width="100%"
      height="100%"
      src="https://blog-cms.openfort.xyz/uploads/unity_advanced_iap_212548bbe7.jpg?updated_at=2024-02-09T10:40:14.394Z"
      alt='Openfort In-App Purchase Advanced Sample'
    />
</div>

## Overview

This sample project showcases the Openfort advanced integration with [In-App Purchasing](https://docs.unity3d.com/Packages/com.unity.purchasing@4.10/manual/Overview.html) in Unity. The objective of this integration sample is to implement and showcase a **crypto In-App Purchasing system** compliant with the [rules/guidelines](https://brandonaaskov.notion.site/The-Apple-Pay-Flow-10ea358d903444298513ac42b1f383d8) companies like Apple have set for this type of purchases in mobile apps.

## Specifications

The sample includes:
  - [**`ugs-backend`**](https://github.com/openfort-xyz/iap-unity-sample/tree/main/ugs-backend)
    
    A .NET Core project with [Cloud Code C# modules](https://docs.unity.com/ugs/en-us/manual/cloud-code/manual/modules#Cloud_Code_C#_modules) that implement [Openfort C# SDK](https://www.nuget.org/packages/Openfort.SDK/1.0.21) methods. Needs to be hosted in Unity Gaming Services.

  - [**`unity-client`**](https://github.com/openfort-xyz/iap-unity-sample/tree/main/unity-client)

    A Unity sample game that connects to ``ugs-backend`` through [Cloud Code](https://docs.unity.com/ugs/manual/cloud-code/manual). It uses [Openfort Unity SDK](https://github.com/openfort-xyz/openfort-csharp-unity) to have full compatibility with `ugs-backend` responses.

## Prerequisites
+ **Get started with Openfort**
  + [Sign in](https://dashboard.openfort.xyz/login) or [sign up](https://dashboard.openfort.xyz/register) and create a new dashboard project

+ **Get started with UGS**
  + [Complete basic prerequisites](https://docs.unity.com/ugs/manual/overview/manual/getting-started#Prerequisites)
  + [Create a project](https://docs.unity.com/ugs/manual/overview/manual/getting-started#CreateProject)

+ **Get started with Google Play Console**
  + [Create and set up your app](https://support.google.com/googleplay/android-developer/answer/9859152?hl=en)

+ **Get started with Apple Developer Account**
  + [Set up everything needed for Apple development](https://developer.apple.com/help/account/)
  + Make sure to [sign the Paid Apps agreement](https://developer.apple.com/help/app-store-connect/manage-agreements/sign-and-update-agreements) and fill tax and banking details as it's needed for testing IAP.

## Setup Openfort dashboard
  
  + [Add an NFT contract](https://dashboard.openfort.xyz/assets/new)
    
    This sample requires an NFT contract to run. We use [0x38090d1636069c0ff1Af6bc1737Fb996B7f63AC0](https://mumbai.polygonscan.com/address/0x38090d1636069c0ff1Af6bc1737Fb996B7f63AC0) (contract deployed in 80001 Mumbai). You can use it for this tutorial too:

    <div align="center">
    <img
      width="50%"
      height="50%"
      src="https://blog-cms.openfort.xyz/uploads/ugs_integration_4_9397f3633b.png?updated_at=2023-12-14T15:59:33.808Z"
      alt='Contract Info'
    />
    </div>
  
  + [Add an ERC20 contract](https://dashboard.openfort.xyz/assets/new)
    
    This sample also requires an ERC20 contract to run. You can [deploy a standard one](https://thirdweb.com/thirdweb.eth/TokenERC20) and then add it to the Openfort dashboard following the same logic as above.

  + [Add a Policy](https://dashboard.openfort.xyz/policies/new)
    
    We aim to cover gas fees for our users when they mint the NFT. Set a new gas policy for that:

    <div align="center">
    <img
      width="50%"
      height="50%"
      src="https://blog-cms.openfort.xyz/uploads/ugs_integration_5_ab3d8ad48d.png?updated_at=2023-12-14T15:59:33.985Z"
      alt='Gas Policy'
    />
    </div>

    Add a rule so the NFT contract uses this policy:

    <div align="center">
    <img
      width="50%"
      height="50%"
      src="https://blog-cms.openfort.xyz/uploads/ugs_integration_6_6727e69146.png?updated_at=2023-12-14T15:59:33.683Z"
      alt='NFT Policy Rule'
    />
    </div>

    Add also a rule for the ERC20 contract:

    <div align="center">
    <img
      width="50%"
      height="50%"
      src="https://blog-cms.openfort.xyz/uploads/iap_sample_22_aec7863428.png?updated_at=2023-12-31T16:02:32.817Z"
      alt='ERC20 Policy Rule'
    />
    </div>

  + [Add a Treasury Developer Account](https://dashboard.openfort.xyz/accounts)

    Enter a name and choose ***Add account***:

    <div align="center">
    <img
      width="50%"
      height="50%"
      src="https://blog-cms.openfort.xyz/uploads/iap_sample_23_74b85444b2.png?updated_at=2023-12-31T16:09:09.921Z"
      alt='Developer account'
    />
    </div>

    This will automatically create a custodial wallet that we'll use to transfer the ERC20 tokens to the players. **IMPORTANT: Transfer a good amount of tokens from the created ERC20 contract to this wallet to facilitate testing**.

  + [Add a Minting Developer Account](https://dashboard.openfort.xyz/accounts)

    Enter a name and choose ***Add account***:

    <div align="center">
    <img
      width="50%"
      height="50%"
      src="https://blog-cms.openfort.xyz/uploads/iap_sample_23_74b85444b2.png?updated_at=2023-12-31T16:09:09.921Z"
      alt='Minting Developer account'
    />
    </div>

    This will automatically create a custodial wallet that the players will transfer the NFTs to when they choose the sell them. They'll get rewarded from the Treasury Dev Account afterwards.

## Set up [`ugs-backend`](https://github.com/openfort-xyz/iap-unity-sample/tree/main/ugs-backend)

- ### Set Openfort dashboard variables

  Open the [solution](https://github.com/openfort-xyz/iap-unity-sample/blob/main/ugs-backend/CloudCodeModules.sln) with your preferred IDE, open [``SingletonModule.cs``](https://github.com/openfort-xyz/iap-unity-sample/blob/main/ugs-backend/CloudCodeModules/SingletonModule.cs) and fill in these variables:

  <div align="center">
    <img
      width="50%"
      height="50%"
      src="https://blog-cms.openfort.xyz/uploads/unity_iap_advanced_sample1_9ab5bedd91.png?updated_at=2024-02-13T23:41:17.097Z"
      alt='Singleton Module'
    />
    </div>

  - `OfApiKey`: [Retrieve the **Openfort secret key**](https://dashboard.openfort.xyz/apikeys)
  - `OfNftContract`: [Retrieve the **NFT contract API ID**](https://dashboard.openfort.xyz/assets)
  - `OfGoldContract`: [Retrieve the **ERC20 contract API ID**](https://dashboard.openfort.xyz/assets)
  - `OfSponsorPolicy`: [Retrieve the **Policy API ID**](https://dashboard.openfort.xyz/policies)
  - `OfDevTreasuryAccount`: [Retrieve the **Treasury Developer Account API ID**](https://dashboard.openfort.xyz/accounts)
  - `OfDevMintingAccount`: [Retrieve the **Minting Developer Account API ID**](https://dashboard.openfort.xyz/accounts)

- ### Package Code
  Follow [the official documentation steps](https://docs.unity.com/ugs/en-us/manual/cloud-code/manual/modules/getting-started#Package_code).

- ### Deploy to UGS
  Follow [the official documentation steps](https://docs.unity.com/ugs/en-us/manual/cloud-code/manual/modules/getting-started#Deploy_a_module_project).

- ### Add a currency to UGS project
  Follow [the official documentation steps](https://docs.unity.com/ugs/en-us/manual/economy/manual/add-currency) to add a currency to your game:

  <div align="center">
    <img
      width="50%"
      height="50%"
      src="https://blog-cms.openfort.xyz/uploads/unity_iap_advanced_sample2_7de4fc0554.png?updated_at=2024-02-13T23:51:37.496Z"
      alt='UGS currency'
    />
    </div>

## Set up [``unity-client``](https://github.com/openfort-xyz/iap-unity-sample/tree/main/unity-client)

In Unity go to *Edit --> Project Settings --> Services* and link the ``unity-client`` to your UGS Project:

<div align="center">
    <img
      width="50%"
      height="50%"
      src="https://blog-cms.openfort.xyz/uploads/iap_sample_16_c1ac7c4b45.png?updated_at=2023-12-28T15:52:03.478Z"
      alt='Services settings'
    />
</div>

Select your *Environment*:

<div align="center">
    <img
      width="50%"
      height="50%"
      src="https://blog-cms.openfort.xyz/uploads/iap_sample_17_e60d56f379.png?updated_at=2023-12-28T15:52:03.577Z"
      alt='UGS environment'
    />
</div>

Now make sure *In-App Purchasing* is enabled and *Current Targeted Store* is set to ***Google Play***. Then follow the instructions to set the **Google Play License Key** to your UGS project:

<div align="center">
    <img
      width="50%"
      height="50%"
      src="https://blog-cms.openfort.xyz/uploads/iap_sample_18_3c78605c09.png?updated_at=2023-12-28T15:52:03.586Z"
      alt='Google Play License Key'
    />
</div>

Your UGS project dashboard should look like this:

<div align="center">
    <img
      width="50%"
      height="50%"
      src="https://blog-cms.openfort.xyz/uploads/iap_sample_19_6802cc268e.png?updated_at=2023-12-28T15:52:04.490Z"
      alt='License key in UGS dashboard'
    />
</div>

**Apple AppStore** doesn't need this *license key configuration* so if you're targeting **iOS** you're good to go. 

## Android deployment

  + ### Build App Bundle

    In Unity go to [*Android Player settings*](https://docs.unity3d.com/Manual/class-PlayerSettingsAndroid.html) and make sure *Other Settings* looks like this:

    <div align="center">
        <img
          width="50%"
          height="50%"
          src="https://blog-cms.openfort.xyz/uploads/iap_sample_7_e6ec7eb903.png?updated_at=2023-12-28T07:47:59.386Z"
          alt='Android Player settings'
        />
    </div>

    Also, make sure to sign the application with a [Keystore](https://docs.unity3d.com/Manual/android-keystore-create.html) in *Publishing Settings*:

    <div align="center">
        <img
          width="50%"
          height="50%"
          src="https://blog-cms.openfort.xyz/uploads/iap_sample_8_ecae38df0e.png?updated_at=2023-12-28T07:47:59.307Z"
          alt='Application Signing'
        />
    </div>

    Then go to *Build Settings*, check ***Build App Bundle (Google Play)*** and choose ***Build***:

    <div align="center">
        <img
          width="50%"
          height="50%"
          src="https://blog-cms.openfort.xyz/uploads/iap_sample_9_6d1e1a5636.png?updated_at=2023-12-28T07:52:15.586Z"
          alt='Build'
        />
    </div>

  + ### Set up Google Play Console

    - #### Create internal release

      On your [Google Play Console](https://play.google.com/console/u/0/developers/7556582789169418933?onboardingflow=signup) app, go to *Release --> Testing --> Internal testing --> Testers* and select or create an email list with the emails that will test your app. Then choose ***Create new release***:

      <div align="center">
          <img
            width="50%"
            height="50%"
            src="https://blog-cms.openfort.xyz/uploads/iap_sample_10_f700f82ef1.png?updated_at=2023-12-28T15:07:32.491Z"
            alt='New release'
          />
      </div>

      Upload the `.aab` file and then choose ***Next***:

      <div align="center">
          <img
            width="50%"
            height="50%"
            src="https://blog-cms.openfort.xyz/uploads/iap_sample_11_06459575df.png?updated_at=2023-12-28T15:07:33.382Z"
            alt='Upload build'
          />
      </div>

      If needed, solve pending errors and warnings and then choose ***Save and publish***:

      <div align="center">
          <img
            width="50%"
            height="50%"
            src="https://blog-cms.openfort.xyz/uploads/iap_sample_12_da52624cb6.png?updated_at=2023-12-28T15:07:32.481Z"
            alt='Save and publish'
          />
      </div>
    
    - #### Import IAP catalog

      On your [Google Play Console](https://play.google.com/console/u/0/developers/7556582789169418933?onboardingflow=signup) app, go to *Monetize --> Products --> In-app products* and choose ***Import***:

      <div align="center">
          <img
            width="50%"
            height="50%"
            src="https://blog-cms.openfort.xyz/uploads/iap_sample_13_04a57f78c6.png?updated_at=2023-12-28T15:07:32.484Z"
            alt='Create product'
          />
      </div>

      Upload the [``GooglePlayProductCatalog.csv``](https://github.com/openfort-xyz/iap-unity-sample/blob/main/unity-client/Assets/GooglePlayProductCatalog.csv) file (which contains all the in-app products) and choose ***Import***:

      <div align="center">
            <img
              width="50%"
              height="50%"
              src="https://blog-cms.openfort.xyz/uploads/iap_sample_14_9a114af583.png?updated_at=2023-12-28T15:07:32.397Z"
              alt='Import products'
            />
      </div>

      You should see all the products have been created:

      <div align="center">
            <img
              width="50%"
              height="50%"
              src="https://blog-cms.openfort.xyz/uploads/iap_sample_15_45877d642d.png?updated_at=2023-12-28T15:07:32.278Z"
              alt='Products created'
            />
      </div>

  + ### Testing

    Once the internal testing release is published, you have two options to test:

    - Build and run the .apk directly to your device ([if the *version number* is the same as in the internal release](https://docs.unity3d.com/Packages/com.unity.purchasing@4.10/manual/Testing.html)).
    - Download the app from Google Play through the internal testing link:

    <div align="center">
          <img
            width="50%"
            height="50%"
            src="https://blog-cms.openfort.xyz/uploads/iap_sample_21_f41d2c851f.png?updated_at=2023-12-28T16:06:28.194Z"
            alt='Internal testing link'
          />
      </div>

## iOS deployment

  + ### Xcode: Build & Archive & Upload
    In Unity go to *File --> Build Settings* and choose ***Build And Run***:

    <div align="center">
          <img
            width="50%"
            height="50%"
            src="https://blog-cms.openfort.xyz/uploads/unity_iap_advanced_sample3_40c42ce5e4.png?updated_at=2024-02-14T00:26:54.820Z"
            alt='Build to Xcode'
          />
      </div>

    This will automatically open Xcode. If you encounter a `signing error`, select your development team and enable ***Automatically manage signing***:

    <div align="center">
          <img
            width="50%"
            height="50%"
            src="https://blog-cms.openfort.xyz/uploads/unity_iap_advanced_sample4_9e12130585.png?updated_at=2024-02-14T00:34:32.096Z"
            alt='Build to Xcode: error'
          />
      </div>

      Start the building process again **(cmd + B)** and when completed, go to ***Product --> Archive***:

      <div align="center">
          <img
            width="50%"
            height="50%"
            src="https://blog-cms.openfort.xyz/uploads/unity_iap_advanced_sample5_f10cfb74a1.png?updated_at=2024-02-14T00:41:32.549Z"
            alt='Build to Xcode: archive'
          />
      </div>

      After completing, choose ***Distribute App***:

      <div align="center">
          <img
            width="50%"
            height="50%"
            src="https://blog-cms.openfort.xyz/uploads/unity_iap_advanced_sample6_85f8e46b54.png?updated_at=2024-02-14T00:47:23.087Z"
            alt='Build to Xcode: Distribute app'
          />
      </div>

      Select ***TestFlight & App Store*** to enable both internal and external testing and choose ***Distribute***:

      <div align="center">
          <img
            width="50%"
            height="50%"
            src="https://blog-cms.openfort.xyz/uploads/unity_iap_advanced_sample7_7b3068a974.png?updated_at=2024-02-14T01:15:28.984Z"
            alt='Build to Xcode: distribute'
          />
      </div>

      The app will be uploaded to **App Store Connect**:

      <div align="center">
          <img
            width="50%"
            height="50%"
            src="https://blog-cms.openfort.xyz/uploads/unity_iap_advanced_sample8_f5a647899b.png?updated_at=2024-02-14T01:15:28.680Z"
            alt='Build to Xcode: complete'
          />
      </div>

  + ### Set up App Store Connect app
    
    Go to [App Store Connect Apps](https://appstoreconnect.apple.com/apps), choose your newly uploaded app and under **Distribution --> In-App Purchases** add the purchases:

    <div align="center">
        <img
          width="50%"
          height="50%"
          src="https://blog-cms.openfort.xyz/uploads/unity_iap_advanced_sample9_8bfb82b934.png?updated_at=2024-02-14T01:15:30.097Z"
          alt='Build to Xcode: in-app purchases'
        />
    </div>

    Remember to fill the same *Product ID* as you have set in your **Unity IAP Catalog**. Do it for all your products:

    <div align="center">
        <img
          width="50%"
          height="50%"
          src="https://blog-cms.openfort.xyz/uploads/unity_iap_advanced_sample10_bace0d3453.png?updated_at=2024-02-14T01:15:29.278Z"
          alt='Build to Xcode: catalog'
        />
    </div>

    Go to the *TestFlight section* and choose ***Manage Missing Compliance*** for your build:

    <div align="center">
        <img
          width="50%"
          height="50%"
          src="https://blog-cms.openfort.xyz/uploads/unity_iap_advanced_sample11_c1f23d62a9.png?updated_at=2024-02-14T01:15:29.089Z"
          alt='Build to Xcode: catalog'
        />
    </div>
  
  + ### Testing

    Go to *Internal Testers* ([add testers](https://developer.apple.com/help/app-store-connect/test-a-beta-version/add-testers-to-builds/)) and you should see your build ready to be tested:

    <div align="center">
          <img
            width="50%"
            height="50%"
            src="https://blog-cms.openfort.xyz/uploads/unity_iap_advanced_sample12_2db9d9b6f4.png?updated_at=2024-02-14T01:15:29.491Z"
            alt='Build to Xcode: testers'
          />
      </div>

    Open the TestFlight in the iOS device where your tester Apple ID is configured and test the app!

## Conclusion

Upon completing the above steps, your Unity game will be fully integrated with Openfort and Unity In-App Purchasing service. Always remember to test every feature before deploying to guarantee a flawless player experience.

For a deeper understanding of the underlying processes, check out the [tutorial video](https://www.youtube.com/watch?v=37yAu7YQXhg). 

## Get support
If you found a bug or want to suggest a new [feature/use case/sample], please [file an issue](https://github.com/openfort-xyz/samples/issues).

If you have questions, or comments, or need help with code, we're here to help:
- on Twitter at https://twitter.com/openfortxyz
- on Discord: https://discord.com/invite/t7x7hwkJF4
- by email: support+youtube@openfort.xyz
