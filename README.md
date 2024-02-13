# Openfort In-App Purchase Advanced Sample

<div align="center">
    <img
      width="100%"
      height="100%"
      src="https://blog-cms.openfort.xyz/uploads/unity_advanced_iap_212548bbe7.jpg?updated_at=2024-02-09T10:40:14.394Z"
      alt='Openfort In-App Purchase Advanced Sample'
    />
</div>

## [Try it live!]()

## Overview

This sample project showcases the Openfort advanced integration with [In-App Purchasing](https://docs.unity3d.com/Packages/com.unity.purchasing@4.10/manual/Overview.html) in Unity. The objective of this integration sample is to implement and showcase a **crypto In-App Purchasing system** compliant with the [rules/guidelines](https://brandonaaskov.notion.site/The-Apple-Pay-Flow-10ea358d903444298513ac42b1f383d8) companies like Apple have set for this type of purchases in mobile apps.

## Workflow

// TODO image

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
  + Make sure to [sign the Paid Apps agreement](https://developer.apple.com/help/app-store-connect/manage-agreements/sign-and-update-agreements) as it's needed for testing IAP.

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

    //TODO add two developer accounts
  + [Add a Developer Account](https://dashboard.openfort.xyz/accounts)

    Enter a name and choose ***Add account***:

    <div align="center">
    <img
      width="50%"
      height="50%"
      src="https://blog-cms.openfort.xyz/uploads/iap_sample_23_74b85444b2.png?updated_at=2023-12-31T16:09:09.921Z"
      alt='Developer account'
    />
    </div>

    This will automatically create a custodial wallet that we'll use to send the ERC20 tokens to the players. **IMPORTANT: Transfer a good amount of tokens from the created ERC20 contract to this wallet to facilitate testing**.

## Set up [`ugs-backend`](https://github.com/openfort-xyz/iap-unity-sample/tree/main/ugs-backend)

- ### Set Openfort dashboard variables

  Open the [solution](https://github.com/openfort-xyz/iap-unity-sample/blob/main/ugs-backend/CloudCodeModules.sln) with your preferred IDE, open [``SingletonModule.cs``](https://github.com/openfort-xyz/iap-unity-sample/blob/main/ugs-backend/CloudCodeModules/SingletonModule.cs) and fill in these variables:

  //TODO
  <div align="center">
    <img
      width="50%"
      height="50%"
      src="https://blog-cms.openfort.xyz/uploads/iap_sample_2_b14ed6e743.png?updated_at=2023-12-31T16:17:12.709Z"
      alt='Singleton Module'
    />
    </div>

  - `OfApiKey`: [Retrieve the **Openfort secret key**](https://dashboard.openfort.xyz/apikeys)
  - `OfNftContract`: [Retrieve the **NFT contract API ID**](https://dashboard.openfort.xyz/assets)
  - `OfGoldContract`: [Retrieve the **ERC20 contract API ID**](https://dashboard.openfort.xyz/assets)
  - `OfSponsorPolicy`: [Retrieve the **Policy API ID**](https://dashboard.openfort.xyz/policies)
  - `OfDevAccount`: [Retrieve the **Developer Account API ID**](https://dashboard.openfort.xyz/accounts)
  - `TODO`: [Retrieve the **Developer Account API ID**](https://dashboard.openfort.xyz/accounts)

- ### Package Code
  Follow [the official documentation steps](https://docs.unity.com/ugs/en-us/manual/cloud-code/manual/modules/getting-started#Package_code).
- ### Deploy to UGS
  Follow [the official documentation steps](https://docs.unity.com/ugs/en-us/manual/cloud-code/manual/modules/getting-started#Deploy_a_module_project).

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

## Test in Editor

Play the **Main** scene and you should see the sign-in panel:

<div align="center">
    <img
      width="50%"
      height="50%"
      src="https://blog-cms.openfort.xyz/uploads/iap_sample_20_8940611454.png?updated_at=2024-01-02T12:23:00.990Z"
      alt='Sign in panel'
    />
</div>

Choose ***Sign in***. The first time it will create a new player but the next time it will sign in as the same player. After some authentication-related logs, this panel should appear:

<div align="center">
    <img
      width="50%"
      height="50%"
      src="https://blog-cms.openfort.xyz/uploads/iap_sample_3_478298f237.png?updated_at=2023-12-27T11:22:46.793Z"
      alt='Game Scene'
    />
</div>

Here you have two options:
+ Purchase ERC20 tokens (x10)
+ Purchase NFT

By clicking any of them, a *Fake Store* panel will pop up, letting you confirm or cancel the purchase:

<div align="center">
    <img
      width="50%"
      height="50%"
      src="https://blog-cms.openfort.xyz/uploads/iap_sample_4_a04e9420ed.png?updated_at=2023-12-27T15:44:43.689Z"
      alt='Game Scene'
    />
</div>

If you confirm, after a brief period you should see the *Transaction successful* message:

<div align="center">
    <img
      width="50%"
      height="50%"
      src="https://blog-cms.openfort.xyz/uploads/iap_sample_5_ac991ecb2a.png?updated_at=2023-12-27T15:48:49.186Z"
      alt='Game Scene'
    />
</div>

You can then click on the inventory icon to see the representation of your on-chain assets:

<div align="center">
    <img
      width="50%"
      height="50%"
      src="https://blog-cms.openfort.xyz/uploads/iap_sample_6_fd93f3f3a2.png?updated_at=2023-12-27T15:51:49.900Z"
      alt='Game Scene'
    />
</div>

In the [Openfort Players dashboard](https://dashboard.openfort.xyz/players), a new player entry should be visible. On selecting this player:

<div align="center">
    <img
      width="50%"
      height="50%"
      src="https://blog-cms.openfort.xyz/uploads/playfab_opensea_img_34_706b0d267e.png?updated_at=2023-11-19T11:06:46.177Z"
      alt='Player Entry'
    />
</div>

You'll notice that a `mint` transaction has been successfully processed:

<div align="center">
    <img
      width="50%"
      height="50%"
      src="https://blog-cms.openfort.xyz/uploads/ugs_integration_7_75cf7a4264.png?updated_at=2023-12-14T16:05:01.500Z"
      alt='Mint Transaction'
    />
</div>

Additionally, by choosing your **Mumbai Account** and viewing ***NFT Transfers***, the transaction is further confirmed:

<div align="center">
    <img
      width="50%"
      height="50%"
      src="https://blog-cms.openfort.xyz/uploads/ugs_integration_8_6b345bd148.png?updated_at=2023-12-14T16:05:00.991Z"
      alt='Etherscan'
    />
</div>

## Build App Bundle

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

## Set up Google Play Console

- ### Create internal release

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
  
- ### Import IAP catalog

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

## Test in Android

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

## Set up AppStore Connect



## Conclusion

Upon completing the above steps, your Unity game will be fully integrated with Openfort and Unity In-App Purchasing service. Always remember to test every feature before deploying to guarantee a flawless player experience.

For a deeper understanding of the underlying processes, check out the [tutorial video](//TODO). 

## Get support
If you found a bug or want to suggest a new [feature/use case/sample], please [file an issue](https://github.com/openfort-xyz/samples/issues)).

If you have questions, or comments, or need help with code, we're here to help:
- on Twitter at https://twitter.com/openfortxyz
- on Discord: https://discord.com/invite/t7x7hwkJF4
- by email: support+youtube@openfort.xyz
