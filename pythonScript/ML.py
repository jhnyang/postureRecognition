import sys
import pickle
from sklearn import svm, metrics

pk1_filename = str(sys.argv[1])
print (pk1_filename)

x_test, y_test = pickle.load(open("test_data.pk1",'rb'))

naive_G, knn, svm_clf, ran_forest, desisionT = pickle.load(open("models.pk1",'rb'))

print("naive")
predict=naive_G.predict(x_test)
score = metrics.accuracy_score(y_test, predict)
report =metrics.classification_report(y_test, predict)
print(score)
print(report)

print("knn")
predict=knn.predict(x_test)
score = metrics.accuracy_score(y_test, predict)
report =metrics.classification_report(y_test, predict)
print(score)
print(report)

print("svm_clf")
predict=svm_clf.predict(x_test)
score = metrics.accuracy_score(y_test, predict)
report =metrics.classification_report(y_test, predict)
print(score)
print(report)

print("ran_forest")
predict=ran_forest.predict(x_test)
score = metrics.accuracy_score(y_test, predict)
report =metrics.classification_report(y_test, predict)
print(score)
print(report)

print("desisionT")
predict=desisionT.predict(x_test)
score = metrics.accuracy_score(y_test, predict)
report =metrics.classification_report(y_test, predict)
print(score)
print(report)