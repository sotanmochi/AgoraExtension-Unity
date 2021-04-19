# Issues

## Audio device issue for iPhone
- AudioをDisableにしているにも関わらず、Agoraによってアプリ内の音声出力に問題が発生する。
  - ChannnelにJoinしている時、アプリ内の音声がスピーカー（Speakerphone）から出力されない。
    通話用のイヤースピーカー（Ear speaker）から出力されてしまい、音量が小さくなってしまう。
  - ChannelからLeaveした後、再生中の音声が聞こえなくなってしまい、再生処理をやり直しても音が出力されない状態が続いてしまう。
  - iPhone 8 Plusで確認済み

- Join処理の実装例は[こちら](https://github.com/sotanmochi/AgoraVideoSamples-Unity/blob/main/AgoraExtension/Runtime/AgoraClient.cs)

- 以下の3つを設定しているが、音声出力の問題が発生する
  - DisableAudio();
  - MuteLocalAudioStream(true);
  - EnableLocalAudio(false);
