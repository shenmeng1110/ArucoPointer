# スキルゼミ課題　SS2409-01
## 課題名：C#によるGUIアプリ開発
氏名：Shen Meng

## 開発環境
<table border="1">
<tr>
<td>環境</td>
<td>バージョン</td>
</tr>
<tr>
<td>Windows10 Visual Studio2022</td>
<td>.NET SDK 9.0.101<br/>Emgu.CV 4.9.0</td>
</tr>
</table>

## 実行条件
``` bash
arucoマークとarrowマークが付けた指示棒
カメラが使えるの設備
```
- ```カメラの選択``` : システムが取得したデバイスリストから使用したいカメラデバイスを選択します。　
- ```カメラのキャリブレーション``` : システムの指示に従って、カメラ領域内でインジケーター スティックをゆっくりと動かします。　 
- ```2点の間の距離計算``` : インジケータースティックの矢印を最初の測定点に向けてから、インジケータースティックを 2 番目の測定点に移動します。

## コメント
-  AForge を通じて利用可能なすべてのカメラデバイスを取得し、カメラの MonikerString を取得して、EmguCV インデックス<->MonikerString マッピング テーブルを確立します。
- arucoマークをリアルタイムに検出し、画面に表示します。
- 初期化したarucoマークと実際に使用するarucoマーク、サイズ、ラベル等の情報が一致していないとエラーが発生します。
- 不必要なメモリを長時間占有しないように、キャリブレーション後は速やかにメモリ空間を解放します。

## 参考資料
- <a href="[https://zhuanlan.zhihu.com/p/112176670](https://github.com/opencv/opencv_contrib/tree/4.0.1/modules/aruco)">opencv modules aruco file
- <a href="[https://blog.csdn.net/weixin_43581819/article/details/124822521](https://www.cnblogs.com/yilangUAV/p/14436171.html)">[OpenCV] aruco マーカー認識
- <a href="[https://zhuanlan.zhihu.com/p/540192623](https://gitcode.csdn.net/66ca05cbaa1c2020b35997ea.html?dp_token=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpZCI6MjAxMDE2MiwiZXhwIjoxNzM2NTc3ODE5LCJpYXQiOjE3MzU5NzMwMTksInVzZXJuYW1lIjoibTBfNTYzMjE0ODAifQ.DQpRe8q8rjigfHiixzXu6upv0t6heq1sB0_78__BrdQ&spm=1001.2101.3001.6650.1&utm_medium=distribute.pc_relevant.none-task-blog-2%7Edefault%7Ebaidujs_baidulandingword%7Eactivity-1-137595943-blog-109115407.235%5Ev43%5Epc_blog_bottom_relevance_base3&depth_1-utm_source=distribute.pc_relevant.none-task-blog-2%7Edefault%7Ebaidujs_baidulandingword%7Eactivity-1-137595943-blog-109115407.235%5Ev43%5Epc_blog_bottom_relevance_base3&utm_relevant_index=2
)">Mat、Bitmap、Image データ型間の変換 (OpenCvSharp)
- <a href="[[https://zhuanlan.zhihu.com/p/112176670](https://github.com/opencv/opencv_contrib/tree/4.0.1/modules/aruco)](https://blog.csdn.net/sswss12345/article/details/134921699#:~:text=%E4%BD%9C%E8%80%85%E5%9C%A8%E5%B7%A5%E4%BD%9C%E4%B8%AD%E5%81%B6%E7%84%B6%E6%8E%A5%E8%A7%A6%E5%88%B0%E4%BA%86Emgu%20CV%E8%BF%99%E4%B8%AA%E8%A7%86%E8%A7%89%E5%A4%84%E7%90%86%E5%B0%81%E8%A3%85%E5%8C%85%EF%BC%8C%E5%B9%B6%E5%AF%B9%E5%AE%83%E7%9A%84%E5%85%B7%E4%BD%93%E5%8A%9F%E8%83%BD%E5%81%9A%E4%BA%86%E6%AF%94%E8%BE%83%E5%85%A8%E9%9D%A2%E7%9A%84%E8%AF%95%E9%AA%8C%EF%BC%8C%E4%B8%BA%E4%BA%86%E6%96%B9%E4%BE%BF%E5%B9%BF%E5%A4%A7C#%E7%A8%8B%E5%BA%8F%E5%91%98%E4%B9%9F%E8%83%BD%E6%84%89%E5%BF%AB%E7%9A%84%E4%BD%93%E9%AA%8C%E5%88%B0%E8%A7%86%E8%A7%89%E5%A4%84%E7%90%86%E7%9A%84%E4%B9%90%E8%B6%A3%EF%BC%8C%E6%88%91%E5%86%B3%E5%AE%9A%E9%80%9A%E8%BF%87%E4%B8%80%E7%B3%BB%E5%88%97%E7%9A%84%E6%96%87%E7%AB%A0%E5%92%8C%E4%BB%A3%E7%A0%81%E6%BC%94%E7%A4%BA%EF%BC%8C%E6%9D%A5%E4%B8%80%E6%AD%A5%E6%AD%A5%E7%9A%84%E5%AE%9E)">Emgu CV の紹介と使い方
