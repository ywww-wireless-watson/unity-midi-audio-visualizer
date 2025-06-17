# Unity MIDI & Audio Visualizer

Real-time MIDI and audio visualizer scripts for Unity.

## Demo Video

[![Sample Video](https://img.youtube.com/vi/XbtKUWIOh_0/0.jpg)](https://www.youtube.com/watch?v=XbtKUWIOh_0)

## Features
- MIDI file reading and event visualization (see `MIDIReader.cs`)
- Audio spectrum visualization (see `AudioSpectrum.cs`)
- Audio waveform visualization (see `AudioTime.cs`)
- UI and particle system animation control (see `AnimObjects.cs`)

## Usage

### Import Scripts
Copy the contents into your Unity project.

### Scene Setup
- Create a new Unity scene or use an existing one.
- Add the following GameObjects as needed:
  - **Audio waveform visualization:** Create an empty GameObject, attach the `AudioTime` script, and assign an `AudioSource` and a `Material`. The waveform will be rendered to this material's main texture.
  - **Audio spectrum visualization:** Create an empty GameObject, attach the `AudioSpectrum` script, and assign an `AudioSource`, a `Material` (for the spectrum texture), a secondary `back` Material (whose color will be changed according to the spectrum), and a `ParticleSystemRenderer` (for visualizing energy with particles).
  - **MIDI visualization:** Create an empty GameObject, attach the `MIDIReader` script, and assign an `AudioSource`. Set the `file_name` property to your MIDI file name (without extension `.mid`). Place the MIDI file in your project's `Assets/` folder. Configure the `PList` array size to match the number of MIDI tracks, and populate it with parent GameObjects (one per track). Each parent GameObject should have child GameObjects that will be animated when MIDI notes are played, and these child GameObjects should have the `AnimObjects` component attached.
  - **UI/particle animation:** Attach the `AnimObjects` script to the relevant UI Image or ParticleSystem GameObject. Set the `type` property as needed (0: UI fade, 1: rotation, 2: particle play). 
    - For type 0 and 1, the GameObject must have an `Image` component
    - For type 2, a `ParticleSystem` component.

### Inspector Setup
- **AudioTime**: Assign the required `material` and `source` (AudioSource) references. Adjust `imagex` and `imagey` to control the resolution of the waveform texture.
- **AudioSpectrum**: Assign the required `material`, `source` (AudioSource), `back` material (for color effects), and `PR` (ParticleSystemRenderer) references. Adjust `imagex`, `imagey`, and `numBins` to customize the visualization.
- **MIDIReader**: Set the `file_name` to your MIDI file name (without extension) and assign an `AudioSource`. Configure the `bpm` value to match your MIDI file's tempo. Set up your `PList` array with parent GameObjects for each MIDI track. Each parent should have child GameObjects with `AnimObjects` components that will be activated by MIDI notes.
- **AnimObjects**: Set the `type` property (0: UI fade, 1: rotation, 2: particle play) based on the desired animation effect.

### Play and Test
- Press Play in the Unity Editor to see the visualizations in action.
- Adjust script parameters in the Inspector to customize the behavior.
- For the MIDIReader, you can enable `debugplay` to test without actual audio playback.

### Troubleshooting
- If you see a `NullReferenceException`, check that all required components are assigned and that GameObjects have the correct scripts/components attached.
- For MIDI visualization, ensure your MIDI file is valid and located in the correct folder (`Assets/` directory).
- If MIDI notes aren't triggering animations, verify that:
  1. Your `PList` array is properly populated with parent GameObjects.
  2. Each parent has the correct number of child objects with `AnimObjects` components.
  3. The `bpm` property in MIDIReader matches your MIDI file's tempo.

## License
This project is released under the **MIT License**. See [LICENSE](LICENSE) for details.
