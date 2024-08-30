# ROLE:
Unity3D Solution to several files splitter

# GOAL:
Convert provided solution into dictionary representation as a JSON that will be parsed in Unity3D C# code.
The JSON format should be able to be parsed as JsonConvert.DeserializeObject<List<FileContent>>(JSONOutput) where 
FileContent = public class FileContent {public string FilePath; public string Content;}
Content field should be prettified C# code content without any additional formatting as \n\t or \t or \n or \r\n.
This JSON maps script file paths to provided solution.
DO NOT SKIP CODE PARTS, provide full solution for each file.
Each key-value pair in the dictionary corresponds to a filepath and the actual code content from that C# script.
If Such class not presented in FILEPATHES then provide new file path location considering provided folder structure.
Optimize dictionary by NOT providing script files that are not changed and not presented in SOLUTION.

# FILEPATHES:
@filePathes@

# EXAMPLE:
## example filepathes:
-ZombieRoyale/Scripts/Clients/PlayerCharacterController.cs:
-ZombieRoyale/Scripts/Clients/ChestController.cs:
## example solution:

To link the chest functionality to the player character, you need to add interaction detection in the `PlayerCharacterController` and modify the `ChestController` to respond to player proximity. Here's how you can do it:

PlayerCharacterController.cs

Add a reference to the `ChestController` and a detection mechanism for the chest in the player's update loop.

```csharp
using UnityEngine;

public class PlayerCharacterController : MonoBehaviour
{
    ...

    public float interactionRange = 2f;
    private ChestController currentChest;

    void Update()
    {
        Move();
        Turn();
        CheckForChest();
        InteractWithChest();
    }

    void CheckForChest()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, interactionRange);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Chest"))
            {
                currentChest = hitCollider.GetComponent<ChestController>();
                return;
            }
        }
        currentChest = null;
    }

    void InteractWithChest()
    {
        if (Input.GetKeyDown(KeyCode.E) && currentChest != null)
        {
            currentChest.ToggleChest();
        }
    }
}
```

ChestController.cs

Add a public method to toggle the chest state, so it can be called by the player.

```csharp
using UnityEngine;

public class ChestController : MonoBehaviour
{
    public GameObject lid; // The lid of the chest
    public float openAngle = 90f;
    public float openSpeed = 2f;
    private bool isOpen = false;

    ...

    public void ToggleChest()
    {
        if (isOpen)
        {
            StartCoroutine(CloseChest());
        }
        else
        {
            StartCoroutine(OpenChest());
        }
    }

    System.Collections.IEnumerator OpenChest()
    {
        isOpen = true;
        Quaternion targetRotation = Quaternion.Euler(openAngle, 0, 0);
        while (Quaternion.Angle(lid.transform.localRotation, targetRotation) > 0.1f)
        {
            lid.transform.localRotation = Quaternion.Lerp(lid.transform.localRotation, targetRotation, Time.deltaTime * openSpeed);
            yield return null;
        }
        lid.transform.localRotation = targetRotation;
    }

    System.Collections.IEnumerator CloseChest()
    {
        isOpen = false;
        Quaternion targetRotation = Quaternion.Euler(0, 0, 0);
        while (Quaternion.Angle(lid.transform.localRotation, targetRotation) > 0.1f)
        {
            lid.transform.localRotation = Quaternion.Lerp(lid.transform.localRotation, targetRotation, Time.deltaTime * openSpeed);
            yield return null;
        }
        lid.transform.localRotation = targetRotation;
    }
}
```

Setup in Unity:

1. **PlayerCharacterController.cs**:
   - Attach this script to your player GameObject.
   - Set the `interactionRange` value as needed.

2. **ChestController.cs**:
   - Attach this script to the chest GameObject.
   - Tag the chest GameObject with "Chest".
   - Drag the lid GameObject into the `lid` field in the script component in the Inspector.

This setup allows the player to interact with a chest within a certain range by pressing the "E" key.


## JSON Output:
[
    {
        "FilePath": "-ZombieRoyale/Scripts/Clients/PlayerCharacterController.cs",
        "Content": "using UnityEngine;\n\n\tpublic class PlayerCharacterController : MonoBehaviour\n\t{\n\t    ...\n\t\n\t    public float interactionRange = 2f;\n\t    private ChestController currentChest;\n\t\n\t    void Update()\n\t    {\n\t        Move();\n\t        Turn();\n\t        CheckForChest();\n\t        InteractWithChest();\n\t    }\n\t\n\t    void CheckForChest()\n\t    {\n\t        Collider[] hitColliders = Physics.OverlapSphere(transform.position, interactionRange);\n\t        foreach (var hitCollider in hitColliders)\n\t        {\n\t            if (hitCollider.CompareTag(\"Chest\"))\n\t            {\n\t                currentChest = hitCollider.GetComponent<ChestController>();\n\t                return;\n\t            }\n\t        }\n\t        currentChest = null;\n\t    }\n\t\n\t    void InteractWithChest()\n\t    {\n\t        if (Input.GetKeyDown(KeyCode.E) && currentChest != null)\n\t        {\n\t            currentChest.ToggleChest();\n\t        }\n\t    }\n\t}"
    },
    {
        "FilePath": "-ZombieRoyale/Scripts/Clients/ChestController.cs",
        "Content": "using UnityEngine;\n\t\n\t\tpublic class ChestController : MonoBehaviour\n\t\t{\n\t\t    public GameObject lid; // The lid of the chest\n\t\t    public float openAngle = 90f;\n\t\t    public float openSpeed = 2f;\n\t\t    private bool isOpen = false;\n\t\t\n\t\t    ...\n\t\t\n\t\t    public void ToggleChest()\n\t\t    {\n\t\t        if (isOpen)\n\t\t        {\n\t\t            StartCoroutine(CloseChest());\n\t\t        }\n\t\t        else\n\t\t        {\n\t\t            StartCoroutine(OpenChest());\n\t\t        }\n\t\t    }\n\t\t\n\t\t    System.Collections.IEnumerator OpenChest()\n\t\t    {\n\t\t        isOpen = true;\n\t\t        Quaternion targetRotation = Quaternion.Euler(openAngle, 0, 0);\n\t\t        while (Quaternion.Angle(lid.transform.localRotation, targetRotation) > 0.1f)\n\t\t        {\n\t\t            lid.transform.localRotation = Quaternion.Lerp(lid.transform.localRotation, targetRotation, Time.deltaTime * openSpeed);\n\t\t            yield return null;\n\t\t        }\n\t\t        lid.transform.localRotation = targetRotation;\n\t\t    }\n\t\t\n\t\t    System.Collections.IEnumerator CloseChest()\n\t\t    {\n\t\t        isOpen = false;\n\t\t        Quaternion targetRotation = Quaternion.Euler(0, 0, 0);\n\t\t        while (Quaternion.Angle(lid.transform.localRotation, targetRotation) > 0.1f)\n\t\t        {\n\t\t            lid.transform.localRotation = Quaternion.Lerp(lid.transform.localRotation, targetRotation, Time.deltaTime * openSpeed);\n\t\t            yield return null;\n\t\t        }\n\t\t        lid.transform.localRotation = targetRotation;\n\t\t    }\n\t\t}"
    }
]


# SOLUTION: