# ReFrame for kaguya

輝夜 (kaguya) 用の ReFrame です。アバターに入っている衣装・ギミックのうち **使わないものにチェックを入れるだけで、アップロード時に自動で取り除きます。** 元のアセットは書き換えません。

## 導入

1. VCC に ReFrame のリポジトリを追加します: <https://reframe.illusive-isc.jp/>
2. `ReFrame for kaguya` をプロジェクトに追加します。共通部分の ReFrameCore は一緒に入ります。

## 使い方

1. Hierarchy で kaguya を右クリック → **ReFrame → このアバターに ReFrame を追加**
2. `ReFrame` オブジェクトの Inspector で、使わない項目にチェックを入れます。チェックした項目は Scene と Hierarchy から消えて見えます。
3. いつもどおりアップロードします。

## 選べる項目

- **衣装・姿**: ベレー帽 / アウター / セーラー服 / ストッキング / ローファー / バッグ / 油揚げ / 胸元の開いた衣装 / 髪 (髪型を差し替えるとき) / 下着
- **ギミック**: もちまる / ファイアガン / ペン / ハートガン / 8bit / 白い息 / AFK の炎 / なで / 表情ロック / 噛みつき / 表情差分 / 胸サイズ
- **尻尾・耳**: 尻尾の見た目・揺れ方・本数・大きさ・向き / 耳
- **エモート・姿勢**: エモート / AFK / 立ち・しゃがみ・伏せ・浮遊のポーズ / ロコモーション (ポーズ・身長)
- **ジェスチャー**: ジェスチャー差分 / ハンドアニメーション

## Quest 簡易対応版

Inspector 上部の **「Quest 簡易対応版を作成」** で Quest 用の設定が増えます。Quest で動かないコンポーネント・揺れ物・マテリアルをここで減らし、ビルドターゲットが Android のときに使われます。詳しくは ReFrameCore の README を見てください。

## ライセンス・連絡先

- [MIT License](LICENSE)
- 作者: illusive_isc — <https://x.com/illusive_isc>
- 配布物: <https://illusive-isc.booth.pm/>
