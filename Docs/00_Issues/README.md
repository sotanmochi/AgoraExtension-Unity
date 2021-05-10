# Issues

## Audio device issue for iPhone
#### Issue #2
- ChannelからLeaveした後、再生中の音声がスピーカーフォンから出力されなくなり、音量が小さくなる。

## Fixed issues
#### Issue #1
ChannnelにJoinしている時、アプリ内の音声がiPhoneのスピーカー（Speakerphone）から出力されない。
    通話用のイヤースピーカー（Ear speaker）から出力されてしまい、音量が小さくなってしまう。  
ChannelからLeaveした後、再生中の音声が聞こえなくなってしまい、再生処理をやり直しても音が出力されない状態が続いてしまう。


AgoraClient内で以下の処理を実装して解決済。ChannelにJoinしている間は、音声がスピーカーフォンから出力されている。
```
public async UniTask<bool> Join(AgoraJoinParameters joinParameters, int timeoutSeconds = 30)
{
    ...

    // Audio
    // _RtcEngine.DisableAudio();
    // _RtcEngine.MuteLocalAudioStream(true);
    // _RtcEngine.EnableLocalAudio(false);
    _RtcEngine.SetEnableSpeakerphone(true);

    ...
}

private void OnJoinChannelSuccess(string channelName, uint userId, int elapsed)
{
    _IsJoined = true;
    _RtcEngine.SetAudioSessionOperationRestriction(AUDIO_SESSION_OPERATION_RESTRICTION.AUDIO_SESSION_OPERATION_RESTRICTION_ALL);
    _OnJoinedChannel.OnNext(userId);
}
```