import requests
from pyquery import PyQuery as pq
from wxpusher import WxPusher
import sched
import time
from datetime import datetime

# 使用的变量
# wxpusher的appToke smzdm
appToken='AT_eDOzGJtokCFe4SlIXT3jLxFh63DLGnln'
# userid，实际使用时需要修改
uid=['UID_4EliXi3FtOV9EfYY3DnZLrDWzipr','UID_aRp9P1K7ltnWKU6arhouejXKt0HF']
# 需要查询的景点及门票
travel_name =["多玛乐园","仙螺岛","新南戴河国际娱乐中心","沙雕海洋乐园"]#新南戴河国际娱乐中心
ticket_name =["多玛乐园门票成人票","南戴河仙螺岛门票+往返索道+4项岛上项目票成人票","新南戴河国际娱乐中心门票成人票","沙雕海洋乐园门票+小火车成人票"]
# 用于存储中间结果的字典
dic={}
# 查询用URL
base_url='https://piao.qunar.com/ticket/list.htm?keyword={}&region=&from=mpl_search_suggest'
# 请求头 浏览器类型
headers = {"user-agent": "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/81.0.4044.9 Safari/537.36"}

# 短信发送函数
def sendmsg(content):
    WxPusher.send_message(content,uids=uid,topic_ids=[],token=appToken)

# 查询价格
def queryticket(travel,ticket):
    # 网页的地址。把keyword和page后面的值设置为填充值
    url = base_url.format(travel)
    print(url)

    # 向网址发送请求
    res = requests.get(url,headers=headers).text
    #print(res)
    # 数据初始化
    doc = pq(res)
    # 通过类选择器获取门票内容
    name = doc(".sight_in_ticket .dark_blue")
    price = doc(".sight_in_ticket em")

    # 遍历景点没票
    for x,s in zip(name.items(),price.items()):
        # 将旅游项目信息转换为文本
        name1=x.text()
        price1=s.text()
        print('遍历景点:'+name1+' '+price1)
        if name1==ticket:
            return price1
    return None

schedule = sched.scheduler(time.time, time.sleep)
# 被周期性调度触发的函数
def checkticket(inc):
    print(datetime.now().strftime("%Y-%m-%d %H:%M:%S"))
    for t,p in zip(travel_name,ticket_name):
        time.sleep(inc)
        price=queryticket(t,p)
        if price==None or t==None:
            print('取数据失败')
            continue
        print(t+' '+price)
        # 如果价格不存在，则先初始化
        if p not in dic:
            dic[p]=price
            print('添加价格')
            continue
        # 判断当前价格与已有价格的高低
        if float(price) < float(dic[p]):
            sendmsg(t+" 降价了 "+price)
        else:
            print(t+" 价格没变 "+price)
        dic[p]=price
    schedule.enter(inc, 0, checkticket, (inc,))
# 默认参数60s
def main(inc=60):
    # enter四个参数分别为：间隔事件、优先级（用于同时间到达的两个事件同时执行时定序）、被调用触发的函数，
    # 给该触发函数的参数（tuple形式）
    schedule.enter(0, 0, checkticket, (inc,))
    schedule.run()

main(40)
#price=queryticket(travel_name[0],ticket_name[0])
#print(travel_name[0])
#print(price)

#sendmsg(travel_name[0]+'  '+price)