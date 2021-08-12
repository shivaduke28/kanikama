# KanikamaGI

KanikamaGIはVRChatでの使用を想定した、ライトマップをランタイムで更新する仕組みです。

![KanikamaGI](https://gyazo.com/5bf6d995804a54a2985559b0261ab7c1/raw)

動作環境
- Unity2019.4.29f1
- VRChat SDK3
- [MerlinVR/UdonSharp](https://github.com/MerlinVR/UdonSharp)

使い方

- [カニカマの使い方](https://github.com/shivaduke28/kanikama/wiki/Kanikama%E3%81%AE%E4%BD%BF%E3%81%84%E6%96%B9)

動作確認用ワールド
- [KanikamaGI Test World](https://vrchat.com/home/launch?worldId=wrld_ebb1341f-15b5-4ca6-9f38-575dfb01bf01)


---

特徴

- Udonを使っているのでVRChatで動きます。
- UnityのRealtimeGIを使用しないため、高負荷でも（多分）止まりません。
- 毎フレーム更新できます。
- 通常の静的なGIと併用できます。
- StanardシェーダーにKanikama機能を追加したシェーダーが入っています。


残念なところ

- 光源の数だけライトマップの枚数が増えるため、広いワールドでは非推奨です。
- ライトプローブの動的な更新はできません。
- Bakery未対応（いつか対応します）


光源として使えるもの

- ライト
  - Baked、Mixedに対応しています。
- 発光マテリアル
  - StandardシェーダーのEmissionに対応しています。
- 環境光
  - Ambient Lightの明るさと色を変えることができます。
- モニター
  - 画面を最大で16個のマスに分割して、それぞれを発光マテリアルをつけた板ポリとして扱うことでGIに映像を反映します。


仕組み

1. 事前に光源の数だけライトマップをベイクします。
2. ランタイムではUdonとシェーダーを使って、ベイクした複数のライトマップを合成し、RenderTextureに出力します。
3. RenderTextureは事前にGIを反映されたいオブジェクトのマテリアルに配っておくので、ライティングが更新されます。



仕組み自体はよく知られており、新しいものではありません。KanikamaGIの開発では以下を参考にしました。

- [無　解説 - Imaginantia](https://phi16.hatenablog.com/entry/2021/05/29/204643)
- [ProjectCiAN 制作記｜wata_pj｜note](https://note.com/wata_pj/n/n612f66466313)
- リアルタイムレンダリング 第4版 11章


また、開発では以下のリポジトリとアセットにお世話になりました。
- [esnya/UdonRabbit.Analyze](https://github.com/esnya/UdonRabbit.Analyzer)
- [CyanLaser/CyanEmu](https://github.com/CyanLaser/CyanEmu)
- [TopazChat Player 3.0 - よしたかのBOOTH](https://booth.pm/ja/items/1752066)


---

Links

- [KanikamaGI Dicord](https://discord.gg/eQQuR7Rq)
- [my twitter](https://twitter.com/shiva_duke28)
- pull requests will be very wellcomed <3
