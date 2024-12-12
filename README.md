# CodeGenerator

**Code Generator for Unity using an Agent System**

[Community Discord](https://discord.gg/bWA46Xkv96)

[Tutorial](https://www.youtube.com/watch?v=XRvLZD5PUJk)

---

**Code Generator** simplifies the process of generating and integrating code within your Unity project. This plugin provides an intuitive interface for selecting class names, generating prompts, and utilizing AI APIs to automate code generation.

## Key Features

- **Task Input:** Define the task or prompt for which you need code generation.
- **Class Selection:** Easily search and select classes from your project.
- **Settings Management:** Securely manage API keys for Gemini, OpenAI, Groq, and Anthropic.
- **Code Prompt Generation:** Generate detailed code prompts based on selected classes.
- **Code Generation:** Automate the code generation process using AI agents, with real-time progress updates.
- **Customization:** Add ignored folders to exclude certain directories from class searches.
- **Clipboard and File Save:** Copy generated prompts to the clipboard and save them to files for later use.

## Usage

1. **Open the Code Generator:**  
   Navigate to `Tools > Sanat > CodeGenerator` to open the Code Generator window.

2. **Task Input:**  
   Enter a description of the task for which you need code generation.

3. **Class Selection:**  
   - Click "Refresh Class List" to load available classes.
   - Enter a class name in the search field and select from the filtered suggestions.
   - Add multiple classes as needed.

4. **Settings:**  
   - Toggle settings visibility to manage API keys.
   - Add ignored folders to exclude specific directories.

5. **Generate Prompt:**  
   Click "Generate Prompt" to create a detailed prompt based on the selected classes and task description.

6. **Generate Code:**  
   Click "Generate Code" to start the AI-powered code generation process. Monitor progress with the visual progress bar.

7. **Save and Copy:**  
   Save the generated prompt to a file and copy it to the clipboard for immediate use.

## Installation

1. Download and import the Code Generator package into your Unity project.
2. Ensure you have access to the required AI APIs (Gemini, OpenAI, Groq, Anthropic).
3. Configure your API keys in the settings section of the Code Generator window.

## Requirements

- **Unity Version:** Compatible with Unity 2020.3 and above.
- **APIs:** Access to any/all of Gemini, OpenAI, Groq, and Anthropic APIs.
- **Packages:** UniTask.
- **Platform:** Supports all platforms that Unity supports.
- **Dependencies:** None

## Technical Details

- **Editor Integration:** Fully integrated into the Unity Editor with a dedicated menu item under `Tools > Sanat > CodeGenerator`.
- **API Support:** Compatible with Gemini, OpenAI, Groq, and Anthropic APIs for AI-powered code generation.
- **UI Features:** User-friendly UI for task input, class selection, and settings management.
- **Animation:** Visual feedback for button interactions and progress bars during code generation.
- **Persistent Settings:** Automatically saves selected classes, API keys, and ignored folders between sessions.
- **Prompt Handling:** Generates and formats prompts with clear and concise code inclusion, task descriptions, and saved prompts.
- **Folder Management:** Allows adding and removing ignored folders to refine the class search process.

---

Elevate your Unity development workflow with the **Code Generator**. Automate code generation, maintain high code quality, and enhance productivity with this essential Unity Editor extension.
