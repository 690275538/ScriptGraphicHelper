importClass(java.io.IOException);
importClass(java.net.NetworkInterface);
importClass(java.io.OutputStream);
importClass(java.net.Socket);
importClass(java.net.ServerSocket);

function Int2Bytes(num) {
    let bytes = java.lang.reflect.Array.newInstance(java.lang.Byte.TYPE, 4);
    let arr = new Array(); arr[0] = (num >> 24) & 0xff; arr[1] = (num >> 16) & 0xff; arr[2] = (num >> 8) & 0xff; arr[3] = num & 0xff;
    bytes[0] = arr[0] < 127 ? arr[0] : arr[0] - 256; bytes[1] = arr[1] < 127 ? arr[1] : arr[1] - 256;
    bytes[2] = arr[2] < 127 ? arr[2] : arr[2] - 256; bytes[3] = arr[3] < 127 ? arr[3] : arr[3] - 256;
    return bytes;
}

$settings.setEnabled('foreground_service', true);
if (!requestScreenCapture()) {
    alert("请求截图权限失败");
    exit();
}

var serversocket;
let socket;
try {
    serversocket = new ServerSocket(5678);
    socket = serversocket.accept();
}
catch (error) {
    print(error);
    serversocket.close();
    $settings.setEnabled('foreground_service', false);
    exit();
}
let img = captureScreen();
let bytes = images.toBytes(img);
let stream = socket.getOutputStream();
let info = Int2Bytes(bytes.length);
stream.write(info); stream.write(bytes);
stream.close();
socket.close();
serversocket.close();
$settings.setEnabled('foreground_service', false);
exit();

