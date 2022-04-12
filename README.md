### How to Deploy in Sandbox
##### Pre-Requisites 
- Sandbox Environment: make your own [here](https://signup.microsoft.com/signup?sku=6a4a1628-9b9a-424d-bed5-4118f0ede3fd&ru=https%3A%2F%2Fbusinesscentral.dynamics.com%2FSandbox%2F%3FredirectedFromSignup%3D1)
- Visual Studio Code. Install from Managed Software Center
- In Visual Studio Code: [AL Language Extension](https://marketplace.visualstudio.com/items?itemName=ms-dynamics-smb.al)

##### Steps to Publish/Launch Integration for Testing
1. In Visual Studio Code, open the BaseApp directory
2. Run the program by pressing Command + Shift + P and typing in AL:Publish without Debugging.
3. The Debug console should now inexplicably be open. If not, you can go to View > Debug Console. Upon first Authentication, it will give instructions on logging in with a device code. Use the code provided to activate the VisualStudioCode credentials.
4. Once the Credentials have been cached, the integration will publish the application to the sandbox Business Central Environment and launch the webpage. Open the integration by searching for "Square" in the search field, which can be launched on the top right of the page.
5. Success! The integration is successfully deployed on the sandbox Business Central Dynamics 365 Environment.


##### Common Pitfalls/FAQ
* If the following error occurs: "The target Page "Customer List" for the extension object is not found" --> In VSC, press Command + Shift + P, and type "AL: Download Symbols"
* You may need specific User Privileges to publish extensions. If the extension does not appear to be publishing, use the Search Field in the top right corner of the Sandbox Business Central page, and navigate to the Users view. In there, add the D365 EXTENSION MGT permission set with no specified company (the company field should be left blank). This should allow you to publish the application
* If the following error occurs: "The extension could not be deployed because it is already deployed on another tenant". --> Change the version number in the app.json as well as any other details you'd like to use to separate out the integration from the official integration provided in App Source.

*** 
##### GetAuthorizationCode
[![Deploy to Azure](http://azuredeploy.net/deploybutton.png)](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2Fvkhutornyi%2FOAuthCodeFunctionFSCM%2Fmaster%2Fazuredeploy.json)
