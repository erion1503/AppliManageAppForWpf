AppliManageAppForWpf
=====================

概要
----
AppliManageAppForWpf は Windows 向けの WPF デスクトップアプリで、アプリケーションやショートカットをドラッグ＆ドロップで登録し、アイコンをクリックして起動できるランチャーです。背景シェイプ（四角／円／五角形／六角形）や背景テキスト、背景画像、アイコン配置などを設定できます。

主な機能
-------
- ドラッグ＆ドロップでショートカットや実行ファイルを登録
- アイコンをクリックしてアプリを起動
- アイテムの編集・削除・並べ替え（ドラッグ操作）
- 背景形状の選択（Rectangle / Ellipse / Pentagon / Hexagon）
- 背景テキスト（複数行）、フォント、色、行間、影、位置の設定
- 背景画像（配置モード: Fill / Fit / Tile）と透過度設定
- アイコンサイズとアイコン配置（Top/Center/Bottom/Left/Right）、および自由配置モード（Canvas ベース）
- 設定の永続化（%APPDATA%/AppliManageAppForWpf/settings.xml）
- 設定のエクスポート / インポート（XML）

開発要件
--------
- .NET Framework 4.7.2
- Visual Studio 2019/2022/2026 等で WPF 開発が可能な環境

ビルドと実行
-----------
1. レポジトリをクローンまたは配置
2. Visual Studio でソリューションファイル (AppliManageAppForWpf.slnx) を開く
3. プロジェクトのターゲットフレームワークが .NET Framework 4.7.2 になっていることを確認
4. ビルド → 実行

設定と永続化
-----------
- 設定は XML（settings.xml）で %APPDATA%/AppliManageAppForWpf に保存されます。
- 設定は Settings ウィンドウから編集できます。変更は OK で適用され、Cancel で破棄されます。
- エクスポートは XML（ZIP 形式は未有効化）で行えます。ZIP による追加の実装や再有効化は、プロジェクトに System.IO.Compression.FileSystem 参照を追加して行ってください。

開発メモ
-------
- アイコン抽出には SHGetFileInfo を使用しています（P/Invoke）。
- 自由配置モードでは ItemsControl の ItemsPanel を Canvas に切り替え、AppItem に X/Y プロパティを持たせて配置します。
- SettingsWindow は多数のコントロールをコード側で FindName して初期化・プレビュー更新を行う設計です。

既知の注意点
-----------
- ZIP エクスポートは現在無効化されています。必要ならプロジェクト参照を追加して有効化できます。
- AppListVertical（縦リスト）の自由配置は現状未対応です。

貢献
----
バグ修正や機能追加はプルリクエストで歓迎します。小さな変更でも issue を立ててから作業してください。

ライセンス
--------
リポジトリにライセンスファイルが無い場合は、使用／配布に関してリポジトリ所有者に確認してください。

問い合わせ
---------
実装や動作に関する質問は Issue を作成してください。
