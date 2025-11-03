**Lập trình game với Unity #1** 
**Test đồng bộ 2**
**Hướng dẫn triển khai**

Clone project về máy: 

        git clone https://github.com/XBinh/se07-18.1

**Ý tưởng**

1. Lập trình game đơn giản thể loại match 3+ graphic sprite với Unity
2. Thiết kế được một RESTful API để có thể giao tiếp được với Server
3. Xây dựng Database để nhập thông tin của người chơi
4. Cho phép đăng nhập để lưu thông tin tài khoản người dùng

**Công việc**

1. Chèn TextMesh Pro vào scene (chèn text vào được các đối tượng trong game)

![LivingRoom](https://user-images.githubusercontent.com/91813809/150793809-1da776e7-fa7e-4580-9c50-7c140a6dbff7.png)
![SelectRoom](https://user-images.githubusercontent.com/91813809/150794074-01cda166-5b2f-42b5-9ab1-9a333a2132ad.png)

2. Xây dựng database để có thể thêm được các dữ liệu về người chơi, điểm số

![image](https://user-images.githubusercontent.com/91813809/150797294-b315d130-d718-419b-8e5f-3b09c61ae152.png)

3. Cho phép đăng nhập thông qua nền tảng Facebook

![](https://scontent.fhan3-2.fna.fbcdn.net/v/t1.15752-9/271057135_687840755577432_6437969579046810372_n.png?_nc_cat=107&ccb=1-5&_nc_sid=ae9488&_nc_ohc=Y2JDUkbj_HwAX9mlTrl&tn=79OlbjoCU97hTnLB&_nc_ht=scontent.fhan3-2.fna&oh=03_AVLCHrx3TaAaAD0OK2Vbbr3TD8pccxBty5U0yfUZLHgPoA&oe=62123E01)

**Công nghệ**

1. Sử dụng Yii PHP Framework để code giao tiếp với server
2. Sử dụng Postman để có thể đưa ra các request cho API
3. Xampp để có giao thức truy cập localhost, từ đó tạo ra được cơ sở dữ liệu cho game

**Thành viên**
1. Nguyễn Thị Hải Anh - 19000390
2. Hoàng Thị Ngọc Ánh - 19000393
3. Nguyễn Xuân Bình - 19000398
4. Công Anh Dũng - 19000405
5. Đỗ Ánh Tuyết - 19000486


**Hướng dẫn triển khai**
1. Cài đặt Yii PHP Framework: đầu tiên cần cài đặt composer: có thể cài đặt Composer qua video này: https://www.youtube.com/watch?v=pbxI7I4iC2I&t=1s

sau đó cài đặt Yii PHP Framework qua video này: https://www.youtube.com/watch?v=x8t4mWbmWUc&t=125s

2. Cài đặt Postman để test API. Link cài đặt: https://www.postman.com/downloads/

**Một số hình ảnh minh họa từ game**

![GameScreen](https://user-images.githubusercontent.com/91813809/150795700-4cda3746-9ec6-4d40-8d0e-f912fb3ae649.png)

![TextDialog](https://user-images.githubusercontent.com/91813809/150796168-5dbd98ce-b74e-4412-b193-a98af908803c.png)

![DailyGiftScreen](https://user-images.githubusercontent.com/91813809/150795545-b78547e9-1529-4da2-9fb4-eac9abe1b20e.png)

![GiftBoxScreen](https://user-images.githubusercontent.com/91813809/150795330-3fc992ab-91a4-46c0-9946-e6000fb2a77d.png)

![Rating](https://user-images.githubusercontent.com/91813809/150794462-705b1be7-6c01-461d-ae46-c36e1e3ae2ce.png)

