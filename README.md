**Lập trình game với Unity #1** 

**Hướng dẫn triển khai**

Clone project về máy: 

        git clone https://github.com/XBinh/se07-18.1

**Ý tưởng**

1. Lập trình game đơn giản thể loại match 3+graphic sprite với Unity
2. Thiết kế được một RESTful API để có thể giao tiếp được với Server
3. Xây dựng Database để nhập thông tin của người chơi
4. Cho phép đăng nhập để lưu thông tin tài khoản người dùng

**Công việc**

1. Chèn text vào được các đối tượng trong game

![](https://scontent.fhan3-3.fna.fbcdn.net/v/t1.15752-9/272072701_6975443702497696_6995857094544005055_n.png?_nc_cat=108&ccb=1-5&_nc_sid=ae9488&_nc_ohc=cgdzuZ68fQAAX-B5hXh&_nc_ht=scontent.fhan3-3.fna&oh=03_AVK9Y1SbtAIfbFgHiQiCK3vEOrfg8W8lKuDtm6q0GLuIyA&oe=62126E7F)

3. xây dựng database để có thể thêm được các dữ liệu về người chơi, điểm số

![](https://scontent.xx.fbcdn.net/v/t1.15752-9/p403x403/271794529_387274356498400_5581540350146669389_n.png?_nc_cat=110&ccb=1-5&_nc_sid=aee45a&_nc_ohc=gvtRrNEIkkQAX8Nk4mx&_nc_ad=z-m&_nc_cid=0&_nc_ht=scontent.xx&oh=03_AVJFjsKRLZD7eoHWv2J7st5kj17ZEwMtKlr90bGVlZQTYg&oe=621483A2)

4. cho phép đăng nhập thông qua nền tảng Facebook

![](https://scontent.fhan3-2.fna.fbcdn.net/v/t1.15752-9/271057135_687840755577432_6437969579046810372_n.png?_nc_cat=107&ccb=1-5&_nc_sid=ae9488&_nc_ohc=Y2JDUkbj_HwAX9mlTrl&tn=79OlbjoCU97hTnLB&_nc_ht=scontent.fhan3-2.fna&oh=03_AVLCHrx3TaAaAD0OK2Vbbr3TD8pccxBty5U0yfUZLHgPoA&oe=62123E01)

**Công nghệ**

1. Sử dụng Yii PHP Framework để code giao tiếp với server
2. Sử dụng Postman để có thể đưa ra các request cho API
3. Xampp để có giao thức truy cập localhost, từ đó tạo ra được cơ sở dữ liệu cho game

**Thành viên**
1. Nguyễn Thị Hải Anh - 19000405
2. Hoàng Thị Ngọc Ánh - 19000393
3. Nguyễn Xuân Bình - 19000398
4. Đỗ Ánh Tuyết - 19000486
5. Công Anh Dũng - 19000405

**Hướng dẫn triển khai**
1. Cài đặt Yii PHP Framework: đầu tiên cần cài đặt composer: có thể cài đặt Composer qua video này: https://www.youtube.com/watch?v=pbxI7I4iC2I&t=1s

sau đó cài đặt Yii PHP Framework qua video này: https://www.youtube.com/watch?v=x8t4mWbmWUc&t=125s

2. Cài đặt Postman để test API. Link cài đặt: https://www.postman.com/downloads/

**Một số hình ảnh minh họa từ game**

![](https://scontent.xx.fbcdn.net/v/t1.15752-9/p403x403/271447114_1903357663196233_8966604897880289441_n.png?_nc_cat=104&ccb=1-5&_nc_sid=aee45a&_nc_ohc=cNJZfoaZ3kYAX-phdEq&_nc_oc=AQmhtUOxFVxn8oIt9t05l4qzWGHtHZ0JETzvL7FxlnYGofXpXPuB9CKeWcNAsSbo1gNC15iHUBXduHs-r9M4wxrf&_nc_ad=z-m&_nc_cid=0&_nc_ht=scontent.xx&oh=03_AVK30a_BGayofcIVE3QqIOy4swUtAULsDVj1vrbwC3UgIg&oe=621587F2)

![](https://scontent.xx.fbcdn.net/v/t1.15752-9/p403x403/272111474_478012867275682_1784498890752535630_n.png?_nc_cat=109&ccb=1-5&_nc_sid=aee45a&_nc_ohc=qLVB-o3rvJgAX-H6i3e&_nc_ad=z-m&_nc_cid=0&_nc_ht=scontent.xx&oh=03_AVLqrAJMVLwwVd725g6TrNmiSC_oeJ4vic0BAftZahbrVA&oe=62154755)

![](https://scontent.xx.fbcdn.net/v/t1.15752-9/s720x720/272267606_3166555556954123_8275519720062433774_n.png?_nc_cat=100&ccb=1-5&_nc_sid=aee45a&_nc_ohc=bxWN6Q1WtGQAX9qs485&_nc_ad=z-m&_nc_cid=0&_nc_ht=scontent.xx&oh=03_AVIKLLuXZFwrSPBrK-dACDBzV953RA_vaqOysbnr4d2t1A&oe=6213CAD5)

![](https://scontent.xx.fbcdn.net/v/t1.15752-9/p403x403/271490851_1291214568041634_4935325984936618970_n.png?_nc_cat=106&ccb=1-5&_nc_sid=aee45a&_nc_ohc=juczle_p4UEAX89TfEX&_nc_ad=z-m&_nc_cid=0&_nc_ht=scontent.xx&oh=03_AVK51bygosnj0nqiIih1h2j6S_GUWd5P4aoLxVfEN0gywQ&oe=6214F40B)
