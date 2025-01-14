namespace UnrealEngine.Tests;
public class AudioPlayback : ISystem
{
    public void OnBeginPlay()
    {
        Actor alarmSound = new("AlarmSound");
        AudioComponent alarmAudioComponent = new(alarmSound);
        SoundWave? alarmSoundWave = SoundWave.Load("/Game/Tests/AlarmSound");

        Assert.IsTrue(alarmSoundWave is not null);

        if (alarmSoundWave is not null)
        {
            Debug.AddOnScreenMessage(-1, 5.0f, Color.PowderBlue, "Sound wave duration: " + alarmSoundWave.Duration + " seconds");

            alarmSoundWave.Loop = true;
            alarmAudioComponent.SetSound(alarmSoundWave);
            alarmAudioComponent.Play();

            Assert.IsTrue(alarmAudioComponent.IsPlaying);
        }
    }

    public void OnEndPlay()
    {
        Debug.ClearOnScreenMessages();
    }
}
